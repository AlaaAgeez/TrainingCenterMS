using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingCenter.Core.Consts
{
    public static class JwtSettingsKeys
    {
        public const string SecretKey = "JwtSettings:SecretKey";
        public const string Issuer = "JwtSettings:Issuer";
        public const string Audience = "JwtSettings:Audience";
        public const string AccessTokenExpiryMinutes = "JwtSettings:AccessTokenExpiryMinutes";
        public const string RefreshTokenExpiryDays = "JwtSettings:RefreshTokenExpiryDays";
    }
}