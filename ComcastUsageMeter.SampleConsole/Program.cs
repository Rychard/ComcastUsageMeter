using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ComcastUsageMeter.Shared;
using Newtonsoft.Json;

namespace ComcastUsageMeter.SampleConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            String username = null;
            String password = null;
            Boolean outputJson = false;

            // This is extremely rudimentary solution to an extremely rudimentary issue.
            // I don't anticipate needing to add any additional command-line arguments.
            if (args.Length == 2)
            {
                String[] validOptions = { "-config", "-c" };

                String option = args[0];
                String parameter = args[1];

                if (validOptions.Contains(option.ToLower()) && File.Exists(parameter))
                {
                    try
                    {
                        var configuration = JsonConvert.DeserializeObject<UsageMeterConfiguration>(File.ReadAllText(parameter));
                        username = configuration.Username;
                        password = configuration.Password;
                        outputJson = configuration.OutputJson;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            
            Console.Title = "Comcast Usage Meter";

            if (String.IsNullOrWhiteSpace(username))
            {
                Console.Write("Enter username: ");
                username = Console.ReadLine();
            }

            if (String.IsNullOrWhiteSpace(password))
            {
                Console.Write("Enter password: ");
                password = ReadMaskedPassword();
            }

            if (!outputJson)
            {
                Console.Clear();
                Console.WriteLine("Loading, Please wait...");
            }

            // This is the version used by the official desktop client.
            const String version = "4.0";

            // Sample Response: Authentication
            /* 
            <response>
                <status>
                    <error_code>0</error_code>
                    <error_text/>
                    <update_url_mac>url_to_update_package</update_url_mac>
                    <update_url_pc>url_to_update_package</update_url_pc>
                </status>
                <access_token>really_long_string</access_token>
            </response>
            */

            var authenticateTask = UsageMeterClient.AuthenticateAsync(username, password, version);
            var authenticateResult = authenticateTask.Result;

            String accessToken = authenticateResult.ResponseObject.AccessToken;
            UsageMeterClient client = new UsageMeterClient(username, accessToken, version);

            // Sample Response: Current Usage
            /*
            <response>
                <status>
                    <error_code>0</error_code>
                    <error_text/>
                    <update_url_mac>url_to_update_package</update_url_mac>
                    <update_url_pc>url_to_update_package</update_url_pc>
                </status>
                <device mac="A1:B2:C3:D4:E5:F6">
                    <counter_start>2015-06-01T00:00:00.000Z</counter_start>
                    <counter_end>2015-06-30T23:59:59.000Z</counter_end>
                    <context_code>E</context_code>
                    <usage_total>186</usage_total>
                    <usage_allowable>300</usage_allowable>
                    <usage_remaining>114</usage_remaining>
                    <usage_percent>62</usage_percent>
                    <usage_uom>GB</usage_uom>
                    <minutes_since_last_update>14</minutes_since_last_update>
                    <additional_billable_used>0</additional_billable_used>
                    <additional_billable_included>0</additional_billable_included>
                    <additional_billable_remaining>0</additional_billable_remaining>
                    <additional_billable_percentUsed>0</additional_billable_percentUsed>
                    <additional_billable_grace_amount_exceeded>false</additional_billable_grace_amount_exceeded>
                    <additional_billable_units_per_block>50</additional_billable_units_per_block>
                    <additional_billable_cost_per_block>10.00</additional_billable_cost_per_block>
                    <additional_billable_blocks_used>0</additional_billable_blocks_used>
                </device>
            </response>
            */

            var usageCurrentTask = client.GetUsageCurrentAsync();
            var usageCurrentResult = usageCurrentTask.Result;

            var device = usageCurrentResult.ResponseObject.Device;

            // Determine how many days are left in this usage period.
            // Note: The counter resets at the beginning of each usage period.
            var remainingUsagePeriod = (device.CounterEnd - DateTime.Now);
            var daysRemaining = remainingUsagePeriod.TotalDays;

            // Calculate average daily usage for this usage period.
            var averageDailyUsage = device.UsageTotal / (DateTime.Now - device.CounterStart).TotalDays;

            // Calculate the average usage per day to avoid overage charges.
            var maximumPerDay = device.UsageRemaining / daysRemaining;

            if (outputJson)
            {
                Console.WriteLine(JsonConvert.SerializeObject(usageCurrentResult, Formatting.Indented));
            }
            else
            {
                Console.Clear();
                Console.WriteLine($"MAC Address: {device.MacAddress}");
                Console.WriteLine($"Usage Period: {device.CounterStart:d} - {device.CounterEnd:d}");
                WriteProgressBarLine(device.UsageTotal, device.UsageAllowable, (Console.BufferWidth/2));
                Console.WriteLine($"You have used {device.UsageTotal}{device.UsageUnitOfMeasurement} ({device.UsagePercent}%) of the allotted {device.UsageAllowable}{device.UsageUnitOfMeasurement}.");
                Console.WriteLine($"You have {device.UsageRemaining}{device.UsageUnitOfMeasurement} available for the remainder of the current usage period.");
                Console.WriteLine();
                Console.WriteLine($"Usage will reset in {daysRemaining:0} days.");
                Console.WriteLine();
                Console.WriteLine($"During this usage period, you have used an average of {averageDailyUsage:F2}{device.UsageUnitOfMeasurement} per day.");
                Console.WriteLine($"To avoid overage charges, limit usage to {maximumPerDay:F2}{device.UsageUnitOfMeasurement} per day for the next {daysRemaining:0} days.");

#if DEBUG
                // Keep the console window open if running from visual studio.
                Console.ReadKey(true);
#endif
            }
        }

        private static String ReadMaskedPassword()
        {
            StringBuilder passwordBuilder = new StringBuilder();
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    passwordBuilder.Append(key.KeyChar);
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && passwordBuilder.Length > 0)
                    {
                        String previousPassword = passwordBuilder.ToString();
                        previousPassword = previousPassword.Substring(0, (previousPassword.Length - 1));
                        passwordBuilder = new StringBuilder(previousPassword);
                        Console.Write("\b \b");
                    }
                }
            }
            while (key.Key != ConsoleKey.Enter);
            return passwordBuilder.ToString();
        }

        private static void WriteProgressBarLine(int percentComplete, int widthOfBar, Boolean centered = true)
        {
            Console.WriteLine();

            // Store the original color so we can revert back upon completion.
            var originalColor = Console.BackgroundColor;

            // Clamp the percentage to ensure it is no more than 100.
            percentComplete = (percentComplete > 100) ? 100 : percentComplete;

            int maximumRight = (Console.BufferWidth - 1);

            int left = Console.CursorLeft;
            int right = (widthOfBar < 1) ? maximumRight : (left + widthOfBar);

            // Clamp the right value to ensure it doesn't extend past the console's buffer.
            right = (right > maximumRight) ? maximumRight : right;

            // The number of individual chunks we can visually display in the bar.
            // NOTE: Subtract 2 to account for the brackets.
            int progressBarChunks = (right - left);

            // What percentage does each chunk represent?
            Double progressPerChunk = (progressBarChunks / 100D);

            // How many of our chunks are roughly equivalent to the current progress?
            int completedChunks = (int)Math.Round((progressPerChunk * percentComplete), 0);

            // Should the progress bar be rendered in the center of the line?
            if (centered)
            {
                // To center it, we just find the total unused horizontal space, and add half of it to the left index.
                int availableSpace = (Console.BufferWidth - progressBarChunks);
                left += (availableSpace / 2);
            }

            // Write the contents of the progress bar.
            Console.CursorLeft = left;
            
            // We'll start out by drawing the filled portion, so we set our background color to green.
            Console.BackgroundColor = ConsoleColor.Green;
            for (int i = 0; i < progressBarChunks; i++)
            {
                if (i >= completedChunks)
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                }
                Console.Write(" ");
            }
            // When we're finished, reset the background color to the original value.
            Console.BackgroundColor = originalColor;
            Console.WriteLine();
            Console.WriteLine();
        }
        private static void WriteProgressBarLine(int currentValue, int maximumValue, int widthOfBar, Boolean centered = true)
        {
            int percentComplete = (int)Math.Round((currentValue / (Double)maximumValue) * 100, 2);
            WriteProgressBarLine(percentComplete, widthOfBar, centered);
        }
    }
}
