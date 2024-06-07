using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Threading.Tasks;
using developers.Models;
using developers.Data;
using Microsoft.Extensions.Configuration;

namespace developers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IDataRepository<User> _userRepository;
        private readonly IDataRepository<Developer> _developerRepository;
        private readonly IConfiguration _configuration;

        public UserController(IDataRepository<User> userRepository, IDataRepository<Developer> developerRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _developerRepository = developerRepository;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _userRepository.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpPost("signup")]
        public async Task<ActionResult<User>> SignUp([FromBody] SignUpRequest signUpRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string passwordHash, passwordSalt;
            CreatePasswordHash(signUpRequest.Password, out passwordHash, out passwordSalt);

            var newUser = new User
            {
                Email = signUpRequest.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Name = signUpRequest.Name,
                Type = "developer",
                Role = "User"
            };

            await _userRepository.AddAsync(newUser);
            await _userRepository.Save();

            // Create a new Developer associated with the user
            var newDeveloper = new Developer
            {
                UserID = newUser.ID,
            };

            await _developerRepository.AddAsync(newDeveloper);
            await _developerRepository.Save();

            return CreatedAtAction(nameof(SignUp), newUser);
        }

[HttpPost("login")]
public async Task<ActionResult<object>> Login(LoginModel loginModel)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    var user = await _userRepository.GetByEmailAsync(loginModel.Email);
    if (user == null)
    {
        return NotFound("Invalid email or password");
    }

    if (!VerifyPasswordHash(loginModel.Password, user.PasswordHash, user.PasswordSalt))
    {
        return BadRequest("Invalid email or password");
    }

    // Retrieve the associated developer for the user
    var developer = await _developerRepository.GetDeveloperByUserIdAsync(user.ID);
    if (developer == null)
    {
        return NotFound("Developer not found");
    }

    // Generate JWT token
    var token = GenerateJwtToken(user);

    // Generate and set refresh token
    var refreshToken = GenerateRefreshToken();
    SetRefreshToken(user, refreshToken);

    var loginResponse = new
    {
        message = "Login successful",
        userId = user.ID,
        developerId = developer.ID, 
        userType = user.Type,
        email = user.Email,
        token,
        userRole = user.Role
    };

    return Ok(loginResponse);
}

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
        {
            if (id != updatedUser.ID)
            {
                return BadRequest("ID in URL does not match the ID in the request body");
            }

            await _userRepository.UpdateAsync(updatedUser);
            var success = await _userRepository.Save();

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            await _userRepository.DeleteAsync(user);
            var success = await _userRepository.Save();

            if (!success)
            {
                return StatusCode(500, "Failed to delete user");
            }

            return NoContent();
        }

        private void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = Convert.ToBase64String(hmac.Key);
                passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            }
        }

        private bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
        {
            using (var hmac = new HMACSHA512(Convert.FromBase64String(storedSalt)))
            {
                var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
                return computedHash == storedHash;
            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)

            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1), // Token expiration time
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(7), 
                Created = DateTime.Now
            };

            return refreshToken;
        }

        private void SetRefreshToken(User user, RefreshToken newRefreshToken)
        {
            // Save refresh token to the user entity
            user.RefreshToken = newRefreshToken.Token;
            user.TokenCreated = newRefreshToken.Created;
            user.TokenExpires = newRefreshToken.Expires;

            // Save changes to the database
            _userRepository.UpdateAsync(user);
            _userRepository.Save();
        }
    }
}
