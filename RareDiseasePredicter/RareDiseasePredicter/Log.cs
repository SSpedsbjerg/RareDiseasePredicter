using System;

namespace RareDiseasePredicter {
    public static class Log {
        public static async Task<bool> LogError(Exception exception, string className, string description) {
            try {
                StreamWriter streamWriter = new StreamWriter("ErrorLog.txt");
                var write = streamWriter.WriteLineAsync($"----------{DateTime.Now:yyyy-MM-dd hh:mm:ss}----------\n{exception}\n{className}\n{description}");
                await write;
                streamWriter.Close();
                return true;
                }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                return false;
                }
            }

        public static async Task<bool> LogWarning(string warning, string className, string description) {
            try {
                StreamWriter streamWriter = new StreamWriter("WarningLog.txt");
                var write = streamWriter.WriteLineAsync($"----------{DateTime.Now:yyyy-MM-dd hh:mm:ss}----------\n{warning}\n{className}\n{description}");
                await write;
                streamWriter.Close();
                return true;
                }
            catch(Exception ex) {
                Console.WriteLine(ex.ToString());
                return false;
                }
            }
        }
    }
