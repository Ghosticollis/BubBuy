using System;
using System.Collections.Generic;
using SDG.Unturned;
using UnityEngine;
using Steamworks;
using System.Text.RegularExpressions;
using System.Collections; // for IEnumerator

namespace BubBuy
{
    class MBubBuy : MonoBehaviour
    {
        public static MBubBuy Instance = null;
        public static Dictionary<int, List<(ushort id, uint price)>> shops = new Dictionary<int, List<(ushort id, uint price)>>();

        public void init() {
            Level.onPostLevelLoaded = (PostLevelLoaded)Delegate.Combine(Level.onPostLevelLoaded, new PostLevelLoaded(MBubBuy.mOnPostLevelLoaded));
        }

        public static void mOnPostLevelLoaded(int level) {
            try {
                if (MBubBuy.Instance == null) {
                    CommandWindow.LogError("BubBuy_error_PIN1005: MBubBuy instance is null, some features might not work!");
                }

                if (level > Level.BUILD_INDEX_SETUP && Provider.isServer) {
                    MConfig.loadConfigData();

                    ChatManager.onChatted += (SteamPlayer stmPlayer, EChatMode mode, ref Color chatted, ref bool isRich, string text, ref bool isVisible) => {
                        try {
                            mOnChatted(stmPlayer, mode, ref chatted, ref isRich, text, ref isVisible);
                        } catch (Exception e) {
                            CommandWindow.LogError("BubBuy_error_PIN1005: exception4 cought: " + e.Message);
                        }
                    };

                    CommandWindow.Log("BubBuy Module is ready ...");
                }
            } catch (Exception e) {
                CommandWindow.LogError("BubBuy_error_PIN1005: exception2 got cought: " + e.Message);
            }
        }

        static void mOnChatted(SteamPlayer stmPlayer, EChatMode mode, ref Color chatted, ref bool isRich, string text, ref bool isVisible) {
            if (text.Length > 1 && text[0] == '/') {
                if (text == "/buy") {
                    isVisible = false;
                    buyFromSign(stmPlayer);
                } else if (stmPlayer.isAdmin && text.StartsWith("/put_sign ")) {
                    ushort id;
                    if (ushort.TryParse(text.Substring(9).Trim(), out id)) {
                        putSellingSign(stmPlayer, id);
                    }
                } else if (stmPlayer.isAdmin && text.StartsWith("/spawn_shop ")) {
                    int id;
                    if (int.TryParse(text.Substring(12).Trim(), out id) && MBubBuy.Instance != null) {
                        ChatManager.say(stmPlayer.playerID.steamID, "spawn shop " + id, Color.yellow);
                        MBubBuy.Instance.StartCoroutine(MBubBuy.Instance.SpawnShop(stmPlayer, id));
                    } else {
                        ChatManager.say(stmPlayer.playerID.steamID, "spawn shop error!", Color.red);
                    }
                } else if (stmPlayer.isAdmin && text == "/list_shops") {
                    foreach (var s in shops.Keys) {
                        ChatManager.say(stmPlayer.playerID.steamID, "shop " + s + ": " + string.Join(",", shops[s]), Color.yellow);
                    }
                }
            }
        }

        IEnumerator SpawnShop(SteamPlayer stmPlayer, int shopId) {
            ChatManager.say(stmPlayer.playerID.steamID, "hmmm", Color.yellow);
            if (shops.ContainsKey(shopId)) {
                ChatManager.say(stmPlayer.playerID.steamID, "after 3 seconds starting to spawn shop " + shopId, Color.yellow);
                yield return new WaitForSeconds(3);
                var items = shops[shopId];
                foreach (var i in items) {
                    putSellingSign(stmPlayer, i.id, i.price);
                    yield return new WaitForSeconds(2);
                }
                ChatManager.say(stmPlayer.playerID.steamID, "done spawning shop " + shopId, Color.yellow);
            } else {
                ChatManager.say(stmPlayer.playerID.steamID, "no shop with this id", Color.red);
            }

        }

        static void putSellingSign(SteamPlayer stmPlayer, ushort id, uint price = 0) {
            ChatManager.say(stmPlayer.playerID.steamID, "spawning a sign with item id:" + id, Color.yellow);
            Ray ray = new Ray(stmPlayer.player.look.aim.position, stmPlayer.player.look.aim.forward);
            RaycastHit m_hit;
            if (Physics.Raycast(ray, out m_hit, 6, RayMasks.BLOCK_BARRICADE) && m_hit.transform != null) {
                CSteamID callerId = stmPlayer.playerID.steamID;
                ItemBarricadeAsset itemBarricadeAsset = (ItemBarricadeAsset)Assets.find(EAssetType.ITEM, 1470);
                if (itemBarricadeAsset != null) {
                    Barricade barricade = new Barricade(1470, itemBarricadeAsset.health, itemBarricadeAsset.getState(), itemBarricadeAsset);
                    BitConverter.GetBytes(callerId.m_SteamID).CopyTo(barricade.state, 0);
                    BitConverter.GetBytes(stmPlayer.playerID.group.m_SteamID).CopyTo(barricade.state, 8);
                    Vector3 point = m_hit.point;// + m_hit.normal * itemBarricadeAsset.offset;
                    var mEulerAngles = Quaternion.LookRotation(m_hit.normal).eulerAngles;
                    Transform t = BarricadeManager.dropBarricade(barricade, m_hit.transform, point, mEulerAngles.x, mEulerAngles.y, mEulerAngles.z, callerId.m_SteamID, stmPlayer.playerID.group.m_SteamID);
                    var sign = t?.GetComponent<InteractableSign>();
                    ItemAsset sell_item = Assets.find(EAssetType.ITEM, id) as ItemAsset;
                    if (sign != null && itemBarricadeAsset != null) {
                        BarricadeManager.ServerSetSignText(sign, @"<color=blue>" + sell_item.itemName + "</color>\nprice:" + price + "\n<color=#1b1b1b>id:" + id + "\n\n_</color>");
                    }

                    itemBarricadeAsset = (ItemBarricadeAsset)Assets.find(EAssetType.ITEM, 1413);
                    if (itemBarricadeAsset != null) {
                        barricade = new Barricade(1413, itemBarricadeAsset.health, itemBarricadeAsset.getState(), itemBarricadeAsset);
                        BitConverter.GetBytes(callerId.m_SteamID).CopyTo(barricade.state, 0);
                        BitConverter.GetBytes(stmPlayer.playerID.group.m_SteamID).CopyTo(barricade.state, 8);
                        point += m_hit.normal * 0.135f + new Vector3(0, -0.3f, 0);// + m_hit.normal * itemBarricadeAsset.offset;
                        mEulerAngles = Quaternion.LookRotation(m_hit.normal).eulerAngles;
                        Transform t2 = BarricadeManager.dropBarricade(barricade, m_hit.transform, point, mEulerAngles.x, mEulerAngles.y, mEulerAngles.z, callerId.m_SteamID, stmPlayer.playerID.group.m_SteamID);
                        InteractableStorage strg = t2?.GetComponent<InteractableStorage>();
                        Item item = new Item(id, EItemOrigin.ADMIN);
                        strg?.items.tryAddItem(item);
                    }
                }
            }
        }

        static void buyFromSign(SteamPlayer stmPlayer) {
            string signText = RaycastGetSignText(stmPlayer);

            CSteamID callerId = stmPlayer.playerID.steamID;
            Player player = stmPlayer.player;
            Match match = Regex.Match(signText, @"price:\s*(\d+).*id:\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success) {
                ushort id;
                byte amount = 1;
                uint price;
                if (uint.TryParse(match.Groups[1].Value, out price) && ushort.TryParse(match.Groups[2].Value, out id)) {
                    if (player.skills.experience >= price) {
                        if (ItemTool.tryForceGiveItem(player, id, amount)) {
                            player.skills.ServerSetExperience(player.skills.experience - price);
                        } else {
                            ChatManager.say(callerId, "failed to get item with this id", Color.red);
                        }
                    } else {
                        ChatManager.say(callerId, "not enough money to buy this item", Color.red);
                    }
                } else {
                    ChatManager.say(callerId, "data error: check syntax, item id and price", Color.red);
                }

            }
        }

        static string RaycastGetSignText(SteamPlayer stmPlayer) {
            CSteamID callerId = stmPlayer.playerID.steamID;
            Ray ray = new Ray(stmPlayer.player.look.aim.position, stmPlayer.player.look.aim.forward);
            RaycastHit m_hit;
            if (Physics.Raycast(ray, out m_hit, 4, RayMasks.BARRICADE_INTERACT)) {
                InteractableSign sign = m_hit.transform.GetComponent<InteractableSign>();
                if (sign != null) {
                    return sign.text;
                }
            }
            return "";
        }
    }
}
