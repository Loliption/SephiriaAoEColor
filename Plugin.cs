using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SephiriaAoEColor
{
    // 敵AoE警告の色を変更する。
    // ゲーム側はほぼ全ての敵攻撃が AOEWarningFactory.HostileWarningColor（getter）で色を取得してから
    // UI_AOEWarning.SetColor → Image.color に1回だけ設定する。この getter の戻り値を差し替える。
    [BepInPlugin(Guid, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "local.sephiria.aoecolor";
        public const string Name = "Sephiria AoE Color";
        public const string Version = "1.2.0";

        // ゲームの元の色（AOEWarningFactory..cctor より）
        internal static readonly Color32 OriginalColor = new Color32(255, 33, 137, 225);

        internal static Plugin Instance;
        internal static BepInEx.Logging.ManualLogSource Log;
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<Color> AoeColor;
        internal static ConfigEntry<UiLanguage> Language;
        internal ConfigEntry<Key> ToggleKey;
        internal ConfigEntry<float> WindowX;
        internal ConfigEntry<float> WindowY;

        private SettingsWindow window;

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            Enabled = Config.Bind("一般", "有効", true, "AoEの色変更を有効にする");
            AoeColor = Config.Bind("一般", "AoEの色", (Color)OriginalColor, "敵AoE警告の色（RRGGBBAA）。最後の2桁は不透明度");
            Language = Config.Bind("一般", "言語", UiLanguage.Auto, "設定画面の言語。Auto はゲームの言語設定に合わせる（日本語/韓国語以外は英語）");
            ToggleKey = Config.Bind("一般", "設定画面キー", Key.F8, "設定画面を開閉するキー");
            WindowX = Config.Bind("ウィンドウ", "X", 40f, "設定画面の位置（1920x1080 基準）");
            WindowY = Config.Bind("ウィンドウ", "Y", 120f, "設定画面の位置（1920x1080 基準）");
            // スライダー操作のたびにファイルを書かないよう、保存は画面を閉じたとき・終了時に行う
            Config.SaveOnConfigSet = false;

            new Harmony(Guid).PatchAll(typeof(Patches));
            Logger.LogInfo($"{Name} {Version} loaded ({ToggleKey.Value} で設定画面)");
        }

        private void Update()
        {
            try
            {
                var kb = Keyboard.current;
                if (kb != null && ToggleKey.Value != Key.None && kb[ToggleKey.Value].wasPressedThisFrame)
                    Toggle();
            }
            catch (Exception) { /* 無効なキー設定など。何もしない */ }

            // 開いている間にゲーム側で言語が変わったら追従する
            if (window != null && window.IsOpen) window.SyncLanguage();
        }

        private void Toggle()
        {
            try
            {
                if (window == null) window = SettingsWindow.Create();
                window.SetOpen(!window.IsOpen);
            }
            catch (Exception e)
            {
                Logger.LogError("設定画面の作成に失敗: " + e);
            }
        }

        internal void SaveConfig() => Config.Save();

        private void OnDestroy()
        {
            Config.Save();
        }
    }

    internal static class Patches
    {
        [HarmonyPatch(typeof(AOEWarningFactory), nameof(AOEWarningFactory.HostileWarningColor), MethodType.Getter)]
        [HarmonyPostfix]
        private static void HostileWarningColor_Postfix(ref Color32 __result)
        {
            if (Plugin.Enabled != null && Plugin.Enabled.Value)
                __result = Plugin.AoeColor.Value;
        }
    }
}
