using Application.Abstractions.Interfaces;
using Application.Abstractions.Interfaces.Repository;
using Application.DTOs.UserDTOs;
using Entities;
using Entities.Models;
using Google.Apis.Auth;

namespace Application.Services
{
    public class AuthorizationService(IUserRepository userRepository, IPasswordHasher passwordHasher, IGoogleTokenValidator googleTokenValidator) : IAuthorizationService
    {
        public async Task<RegisterStatus> RegisterAsync(UserRegisterDTO registerData)
        {
            if (!string.IsNullOrEmpty(registerData.googleJwtToken))
            {
                return await RegisterUserWithGoogleAsync(registerData.googleJwtToken);
            }

            if (!string.IsNullOrEmpty(registerData.Password))
            {
                return await RegisterUserWithPasswordAsync(registerData.Name, registerData.Email, registerData.Password);
            }

            return RegisterStatus.UnknownOathProvider;
        }

        public async Task<LoginStatus> LoginUserAsync(UserLoginDTO loginData)
        {
            if (!string.IsNullOrEmpty(loginData.googleJwtToken))
            {
                return await LoginUserWithGoogleAsync(loginData.googleJwtToken);
            }

            if (!string.IsNullOrEmpty(loginData.Password))
            {
                return await LoginUserWithPasswordAsync(loginData.Email, loginData.Password);
            }

            return LoginStatus.UnknownOathProvider;
        }

        private async Task<RegisterStatus> RegisterUserWithPasswordAsync(string name, string email, string password)
        {
            if (await userRepository.ExistsByEmailAsync(email))
            {
                return RegisterStatus.EmailBusy;
            }

            string passwordHash = passwordHasher.Hash(password);

            if (string.IsNullOrWhiteSpace(name))
            {
                var atIndex = email.IndexOf('@');
                name = atIndex > 0 ? email.Substring(0, atIndex) : email;
            }

            var user = new User
            {
                Username = name,
                Email = email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                Role = "user"
            };

            await userRepository.AddAsync(user);
            return RegisterStatus.Success;
        }

        private async Task<RegisterStatus> RegisterUserWithGoogleAsync(string googleJwtToken)
        {
            try
            {
                var payload = await googleTokenValidator.ValidateAsync(googleJwtToken);
                if (await userRepository.GetByGoogleIdAsync(payload.Subject) != null)
                {
                    return RegisterStatus.EmailBusy;
                }

                var user = new User
                {
                    Username = payload.Name,
                    Email = payload.Email,
                    GoogleId = payload.Subject,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                await userRepository.AddAsync(user);
                return RegisterStatus.Success;
            }
            catch (InvalidJwtException)
            {
                return RegisterStatus.InvalidToken;
            }
        }

        private async Task<LoginStatus> LoginUserWithPasswordAsync(string email, string password)
        {
            if (!await userRepository.ExistsByEmailAsync(email))
            {
                return LoginStatus.IncorrectEmail;
            }

            string passwordHash = passwordHasher.Hash(password);
            bool isPasswordValid = await userRepository.IsPasswordValidByEmailAsync(email, passwordHash);

            return isPasswordValid ? LoginStatus.Success : LoginStatus.IncorrectPassword;
        }

        private async Task<LoginStatus> LoginUserWithGoogleAsync(string googleJwtToken)
        {
            try
            {
                var payload = await googleTokenValidator.ValidateAsync(googleJwtToken);
                if (await userRepository.GetByGoogleIdAsync(payload.Subject) == null)
                {
                    return LoginStatus.UnregisteredGoogle;
                }
                return LoginStatus.Success;
            }
            catch (InvalidJwtException)
            {
                return LoginStatus.InvalidToken;
            }
        }
    }
}