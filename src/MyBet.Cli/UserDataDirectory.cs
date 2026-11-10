namespace MyBet.Cli
{
    public static class UserDataDirectory
    {
        public const string OverrideVariable = "MYBET_HOME";

        public static string Resolve()
        {
            var directory = Environment.GetEnvironmentVariable(OverrideVariable);

            if (string.IsNullOrWhiteSpace(directory))
                directory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyBet");

            Directory.CreateDirectory(directory);
            return directory;
        }
    }
}