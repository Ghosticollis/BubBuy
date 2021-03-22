using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace BubBuy
{
    public class MConfig
    {
        public static void loadConfigData() {
            try {
                string configFile = @".\Modules\BubBuy\config.txt";
                if (System.IO.File.Exists(configFile)) {
                    string[] configData = System.IO.File.ReadAllLines(configFile);
                    foreach (string str in configData) {
                        if (str.Length > 0 && str[0] != '#') {
                            Match match = Regex.Match(str.Trim(), @"^([\.\w\d\s]+):(.+)", RegexOptions.IgnoreCase);
                            if (match.Success) {
                                string option = match.Groups[1].Value.Trim();
                                string data = match.Groups[2].Value.Trim();
                                if (option.Length > 0 && data.Length > 0) {
                                    parseConfigData(option, data);
                                }
                            }
                        }
                    }
                } else {
                    string[] str = { "# lines start with the sign # gonna be ignored", "# " };
                    System.IO.File.WriteAllLines(configFile, str);
                    CommandWindow.LogWarning("BubBuy Module: Can't find config file! auto-created empty config file.");
                }
            } catch (Exception e) {
                CommandWindow.LogError("BubBuy_error_PIN1005: exception3 got cought: " + e.Message);
            }
        }

        static void parseConfigData(string option, string data) {
            if (option == "add shop" ) {
                Match match = Regex.Match(data, @"^(\d+)\s+item_id:\s*(\d+)\s+price:\s*(\d+)", RegexOptions.IgnoreCase);
                if (match.Success) {
                    int shopId;
                    if (int.TryParse(match.Groups[1].Value, out shopId)) {
                        if(!MBubBuy.shops.ContainsKey(shopId)) {
                            MBubBuy.shops.Add(shopId, new List<(ushort id, uint price)>());
                        }
                        MBubBuy.shops[shopId].Add((ushort.Parse(match.Groups[2].Value), uint.Parse(match.Groups[3].Value)));
                    }
                }
            }
        }
    }
}

