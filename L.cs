using System;
using System.Collections.Generic;
using System.Text;

namespace SephiriaAoEColor
{
    public enum UiLanguage { Auto, Japanese, English, Korean }

    // 設定画面の文言（日本語 / 英語 / 韓国語）
    internal static class L
    {
        private const int JA = 0, EN = 1, KO = 2;

        private static readonly Dictionary<string, string[]> Table = new Dictionary<string, string[]>
        {
            //                  日本語                            English                              한국어
            ["title"]   = new[] { "AoE の色設定",                  "AoE Color Settings",                "AoE 색상 설정" },
            ["enable"]  = new[] { "色の変更を有効にする",          "Enable color change",               "색상 변경 사용" },
            ["lang"]    = new[] { "言語",                          "Language",                          "언어" },
            ["auto"]    = new[] { "自動",                          "Auto",                              "자동" },
            ["preview"] = new[] { "プレビュー",                    "Preview",                           "미리보기" },
            ["r"]       = new[] { "赤",                            "Red",                               "빨강" },
            ["g"]       = new[] { "緑",                            "Green",                             "초록" },
            ["b"]       = new[] { "青",                            "Blue",                              "파랑" },
            ["a"]       = new[] { "不透明度",                      "Opacity",                           "불투명도" },
            ["hex"]     = new[] { "カラーコード #",                "Color code #",                      "색상 코드 #" },
            ["reset"]   = new[] { "元の色に戻す",                  "Reset to default",                  "기본 색으로" },
            ["close"]   = new[] { "閉じる",                        "Close",                             "닫기" },
            ["hint"]    = new[] { "{0} で開閉 ・ 次に出るAoEから反映", "{0}: open/close · applies to next AoE", "{0}: 열기/닫기 · 다음 AoE부터 적용" },
        };

        // 言語名はその言語自身の表記で出す
        private static readonly string[] Names = { "日本語", "English", "한국어" };

        private static int idx = EN;
        private static int gameIdx = EN;

        public static int Index => idx;

        // 設定値とゲームの言語コード（"ja-JP" 等）から表示言語を決める。変化したら true
        public static bool Resolve(UiLanguage choice, string gameCode)
        {
            gameIdx = FromGameCode(gameCode);
            int next = choice switch
            {
                UiLanguage.Japanese => JA,
                UiLanguage.English => EN,
                UiLanguage.Korean => KO,
                _ => gameIdx,
            };
            bool changed = next != idx;
            idx = next;
            return changed;
        }

        private static int FromGameCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return EN;
            if (code.StartsWith("ja", StringComparison.OrdinalIgnoreCase)) return JA;
            if (code.StartsWith("ko", StringComparison.OrdinalIgnoreCase)) return KO;
            return EN;
        }

        public static string T(string key) => Table.TryGetValue(key, out var v) ? v[idx] : key;

        public static string ChoiceLabel(UiLanguage choice)
        {
            switch (choice)
            {
                case UiLanguage.Japanese: return Names[JA];
                case UiLanguage.English: return Names[EN];
                case UiLanguage.Korean: return Names[KO];
                default: return $"{T("auto")} ({Names[gameIdx]})";
            }
        }

        // 現在の言語で画面に出る全文字（フォントの対応チェック用）
        public static string UsedChars()
        {
            var sb = new StringBuilder("ONF#0123456789ABCDEFX<>()");
            foreach (var v in Table.Values) sb.Append(v[idx]);
            sb.Append(Names[idx]);
            return sb.ToString();
        }
    }
}
