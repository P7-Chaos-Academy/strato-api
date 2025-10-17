dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=postgres;Port=5432;Database=strato;Username=postgres;Password=postgres;"
dotnet user-secrets set "JwtSettings:SecretKey" "development-secret-key-that-is-long-enough-for-jwt-tokens-to-work-properly-and-secure"
dotnet user-secrets list