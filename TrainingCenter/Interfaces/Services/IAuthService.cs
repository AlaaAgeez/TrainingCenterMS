using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Auth;
using TrainingCenter.Core.DTOs.Common;

namespace TrainingCenter.Core.Interfaces.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<TokenResponseDto>> LoginAsync(LoginRequestDto request);

        Task<ApiResponse<string>> RegisterStaffAsync(RegisterRequestDto request, string adminEmail);

        Task<ApiResponse<string>> RegisterStudentAsync(RegisterStudentDto request);

        Task<ApiResponse<TokenResponseDto>> RefreshTokenAsync(RefreshRequestDto RefreshToken, string Email);

        Task<ApiResponse<string>> LogoutAsync(LogoutRequestDto request, string Email);

        Task<ApiResponse<string>> SendVerificationEmailAsync(ResendVerificationEmailDto request);

        Task<ApiResponse<string>> VerifyEmailAsync(VerifyEmailRequestDto request);

        Task<ApiResponse<string>> ChangePasswordAsync(int userId, ChangePasswordRequestDto request);

        Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto request);

        Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequestDto request);

        Task<ApiResponse<string>> RevokeTokenAsync(int userId, string adminEmail);
    }
}
