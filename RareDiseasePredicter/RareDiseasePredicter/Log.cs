using System;

namespace RareDiseasePredicter {
    public static class Log {
        public static async Task<bool> Error(Exception exception, string className, string description) {
            try {
                StreamWriter streamWriter = new StreamWriter("ErrorLog.txt", append: true);
                var write = streamWriter.WriteLineAsync($"----------{DateTime.Now:yyyy-MM-dd hh:mm:ss}----------\n{exception}\n{className}\n{description}");
                await write;
                streamWriter.Close();
                Console.WriteLine(exception + " "  + className + " " + description);
                return true;
                }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                return false;
                }
            }

        public static async Task<bool> Warning(string warning, string className, string description) {
            try {
                StreamWriter streamWriter = new StreamWriter("WarningLog.txt", append: true);
                var write = streamWriter.WriteLineAsync($"----------{DateTime.Now:yyyy-MM-dd hh:mm:ss}----------\n{warning}\n{className}\n{description}");
                await write;
                streamWriter.Close();
                Console.WriteLine(warning + " "  + className + " " + description);
                return true;
                }
            catch(Exception ex) {
                Console.WriteLine(ex.ToString());
                return false;
                }
            }
        }
    }
