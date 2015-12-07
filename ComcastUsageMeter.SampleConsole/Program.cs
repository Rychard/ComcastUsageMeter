using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ComcastUsageMeter.Shared;
using Newtonsoft.Json;

namespace ComcastUsageMeter.SampleConsole
{
    class Program
    {
        static void Main(String[] args)
        {
            Console.Title = "Comcast Usage Meter";

            UsageMeterConfiguration configuration;

            Boolean configurationLoaded = TryLoadConfiguration(out configuration);

            if (!configurationLoaded)
            {
                // This is extremely rudimentary solution to an extremely rudimentary issue.
                // I don't anticipate needing to add any additional command-line arguments.
                configurationLoaded = TryParseArguments(args, out configuration);
            }

            if (!configurationLoaded)
            {
                configurationLoaded = TryReadFromConsole(out configuration);
            }

            if (!configurationLoaded)
            {
                Console.WriteLine("Configuration could not be performed!");
                return;
            }

            if (!configuration.OutputJson)
            {
                Console.Clear();
                Console.WriteLine("Loading, Please wait...");
            }

            // This is the version used by the official desktop client.
            const String version = "4.1";

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

            var authenticateTask = UsageMeterClient.AuthenticateAsync(configuration.Username, configuration.Password, version);
            var authenticateResult = authenticateTask.Result;

            String accessToken = authenticateResult.ResponseObject.AccessToken;
            UsageMeterClient client = new UsageMeterClient(configuration.Username, accessToken, version);

            // Sample Response: Current Account Usage
            /*
            <response>
                <status>
                    <error_code>0</error_code>
                    <error_text/>
                </status>
                <account ID="1234567890123456">
                    <counter_start>2015-12-01Z</counter_start>
                    <counter_end>2015-12-31Z</counter_end>
                    <usage_total>48</usage_total>
                    <home_usage>48</home_usage>
                    <wifi_usage>0</wifi_usage>
                    <usage_allowable>300</usage_allowable>
                    <overage_usage>0</overage_usage>
                    <usage_remaining>252</usage_remaining>
                    <usage_percent>16.0000</usage_percent>
                    <usage_uom>GB</usage_uom>
                    <minutes_since_last_update>25</minutes_since_last_update>
                    <billable_overage>0</billable_overage>
                    <non_billable_overage>0</non_billable_overage>
                    <additional_billable_used>0</additional_billable_used>
                    <additional_billable_included>0</additional_billable_included>
                    <additional_billable_remaining>0</additional_billable_remaining>
                    <additional_billable_percentUsed>0.0000</additional_billable_percentUsed>
                    <additional_billable_grace_amount_exceeded>false</additional_billable_grace_amount_exceeded>
                    <additional_billable_units_per_block>50</additional_billable_units_per_block>
                    <additional_billable_cost_per_block>10</additional_billable_cost_per_block>
                    <additional_billable_blocks_used>0</additional_billable_blocks_used>
                    <policy_name>300 GB Plan_G5</policy_name>
                    <policy_display_name>Xfinity Data Plan</policy_display_name>
                    <home_device_details>
                        <device>
                            <device_mac>A1:B2:C3:D4:E5:F6</device_mac>
                            <device_usage>48</device_usage>
                            <policy_name>Extreme</policy_name>
                            <policy_context>null</policy_context>
                            <policy_type>Residential</policy_type>
                        </device>
                    </home_device_details>
                </account>
            </response>
            */

            var usageCurrentTask = client.GetUsageAccountCurrentAsync();
            var usageCurrentResult = usageCurrentTask.Result;

            var account = usageCurrentResult.ResponseObject.Account;
            var device = account.HomeDeviceDetails.Device;

            // Determine how many days are left in this usage period.
            // Note: The counter resets at the beginning of each usage period.
            var remainingUsagePeriod = (account.CounterEnd - DateTime.Now);
            var daysRemaining = remainingUsagePeriod.TotalDays;

            // Calculate average daily usage for this usage period.
            var averageDailyUsage = account.UsageTotal / (DateTime.Now - account.CounterStart).TotalDays;

            // Calculate the average usage per day to avoid overage charges.
            var maximumPerDay = account.UsageRemaining / daysRemaining;

            if (configuration.OutputJson)
            {
                Console.WriteLine(JsonConvert.SerializeObject(usageCurrentResult, Formatting.Indented));
            }
            else
            {
                
                Console.Clear();
                Console.WriteLine($"MAC Address: {device.MacAddress}");
                Console.WriteLine($"Usage Period: {account.CounterStart:d} - {account.CounterEnd:d}");
                WriteProgressBarLine(account.UsageTotal, account.UsageAllowable, (Console.BufferWidth/2));
                Console.WriteLine($"You have used {account.UsageTotal}{account.UsageUnitOfMeasurement} ({account.UsagePercent}%) of the allotted {account.UsageAllowable}{account.UsageUnitOfMeasurement}.");
                Console.WriteLine($"You have {account.UsageRemaining}{account.UsageUnitOfMeasurement} available for the remainder of the current usage period.");
                Console.WriteLine();
                Console.WriteLine($"Usage will reset in {daysRemaining:0} days.");
                Console.WriteLine();
                Console.WriteLine($"During this usage period, you have used an average of {averageDailyUsage:F2}{account.UsageUnitOfMeasurement} per day.");
                Console.WriteLine($"To avoid overage charges, limit usage to {maximumPerDay:F2}{account.UsageUnitOfMeasurement} per day for the next {daysRemaining:0} days.");
            }

            if (Debugger.IsAttached)
            {
                // Keep the console window open if running from visual studio.
                Console.ReadKey(true);
            }
        }

        private static Boolean TryReadFromConsole(out UsageMeterConfiguration configuration)
        {
            configuration = new UsageMeterConfiguration();

            Console.Write("Enter username: ");
            configuration.Username = Console.ReadLine();

            Console.Write("Enter password: ");
            configuration.Password = ReadMaskedPassword();

            configuration.OutputJson = false;

            if (String.IsNullOrWhiteSpace(configuration.Username) || String.IsNullOrWhiteSpace(configuration.Username)) { return false; }
            return true;
        }

        private static Boolean TryLoadConfiguration(out UsageMeterConfiguration configuration)
        {
            try
            {
                var configurationFile = ConfigurationManager.AppSettings["ConfigurationFile"];
                var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var configurationFilePath = Path.Combine(currentDirectory, configurationFile);
                if (File.Exists(configurationFilePath))
                {
                    Boolean success = TryDeserializeConfiguration(configurationFilePath, out configuration);
                    if (success) { return true; }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            configuration = null;
            return false;
        }

        private static Boolean TryParseArguments(String[] args, out UsageMeterConfiguration configuration)
        {
            if (args.Length == 2)
            {
                String[] validOptions = { "-config", "-c" };

                String option = args[0];
                String parameter = args[1];

                if (validOptions.Contains(option.ToLower()) && File.Exists(parameter))
                {
                    Boolean success = TryDeserializeConfiguration(parameter, out configuration);
                    if (success) { return true; }
                }
            }
            configuration = null;
            return false;
        }

        private static Boolean TryDeserializeConfiguration(String filename, out UsageMeterConfiguration configuration)
        {
            try
            {
                var configurationData = File.ReadAllText(filename);
                configuration = JsonConvert.DeserializeObject<UsageMeterConfiguration>(configurationData);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                configuration = null;
                return false;
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

        private static void WriteProgressBarLine(Double percentComplete, Double widthOfBar, Boolean centered = true)
        {
            WriteProgressBarLine((Int32)percentComplete, (Int32)widthOfBar, centered);
        }

        private static void WriteProgressBarLine(Int32 percentComplete, Int32 widthOfBar, Boolean centered = true)
        {
            Console.WriteLine();

            // Store the original color so we can revert back upon completion.
            var originalColor = Console.BackgroundColor;

            // Clamp the percentage to ensure it is no more than 100.
            percentComplete = (percentComplete > 100) ? 100 : percentComplete;

            Int32 maximumRight = (Console.BufferWidth - 1);

            Int32 left = Console.CursorLeft;
            Int32 right = (widthOfBar < 1) ? maximumRight : (left + widthOfBar);

            // Clamp the right value to ensure it doesn't extend past the console's buffer.
            right = (right > maximumRight) ? maximumRight : right;

            // The number of individual chunks we can visually display in the bar.
            // NOTE: Subtract 2 to account for the brackets.
            Int32 progressBarChunks = (right - left);

            // What percentage does each chunk represent?
            Double progressPerChunk = (progressBarChunks / 100D);

            // How many of our chunks are roughly equivalent to the current progress?
            Int32 completedChunks = (Int32)Math.Round((progressPerChunk * percentComplete), 0);

            // Should the progress bar be rendered in the center of the line?
            if (centered)
            {
                // To center it, we just find the total unused horizontal space, and add half of it to the left index.
                Int32 availableSpace = (Console.BufferWidth - progressBarChunks);
                left += (availableSpace / 2);
            }

            // Write the contents of the progress bar.
            Console.CursorLeft = left;
            
            // We'll start out by drawing the filled portion, so we set our background color to green.
            Console.BackgroundColor = ConsoleColor.Green;
            for (Int32 i = 0; i < progressBarChunks; i++)
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

        private static void WriteProgressBarLine(Double currentValue, Double maximumValue, Double widthOfBar, Boolean centered = true)
        {
            WriteProgressBarLine((Int32) currentValue, (Int32) maximumValue, (Int32) widthOfBar, centered);
        }

        private static void WriteProgressBarLine(Int32 currentValue, Int32 maximumValue, Int32 widthOfBar, Boolean centered = true)
        {
            Int32 percentComplete = (Int32)Math.Round((currentValue / (Double)maximumValue) * 100, 2);
            WriteProgressBarLine(percentComplete, widthOfBar, centered);
        }
    }
}
