using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SephiriaAoEColor
{
    // ゲーム標準UI風の設定画面（uGUI + ゲーム本体のフォント）。
    // 配色・寸法は SephiriaMeter の設定画面に合わせている。
    internal class SettingsWindow
    {
        private const float Width = 320f;
        private const float HeadH = 26f;
        private const float RowH = 26f;
        private const float Pad = 10f;

        private static readonly Color PanelColor = new Color(0.05f, 0.06f, 0.09f, 0.97f);
        private static readonly Color HeadColor = new Color(0.11f, 0.13f, 0.19f, 0.98f);
        private static readonly Color TextColor = new Color(0.92f, 0.93f, 0.96f, 1f);
        private static readonly Color SubColor = new Color(0.55f, 0.62f, 0.78f, 1f);
        private static readonly Color ButtonColor = new Color(1f, 1f, 1f, 0.1f);

        private GameObject root;
        private Canvas canvas;
        private RectTransform panel;
        private readonly List<TMP_Text> texts = new List<TMP_Text>();
        // 言語切替で書き換える文言（テキスト, キー）
        private readonly List<KeyValuePair<TMP_Text, string>> keyed = new List<KeyValuePair<TMP_Text, string>>();
        private TMP_Text langText;
        private TMP_Text hintText;

        private TMP_Text enabledText;
        private Image swatch;
        private readonly Slider[] sliders = new Slider[4];
        private readonly TMP_Text[] sliderValues = new TMP_Text[4];
        private TMP_InputField hexField;
        private bool suppress;

        public bool IsOpen => root != null && root.activeSelf;

        public static SettingsWindow Create()
        {
            var w = new SettingsWindow();
            w.Build();
            return w;
        }

        public void SetOpen(bool open)
        {
            if (root == null) return;
            if (open)
            {
                SyncLanguage(true);
                canvas.sortingOrder = SortingOrderBelowCursor();
                Refresh();
                root.SetActive(true);
            }
            else
            {
                root.SetActive(false);
                Deselect();
                var p = Plugin.Instance;
                p.WindowX.Value = panel.anchoredPosition.x;
                p.WindowY.Value = -panel.anchoredPosition.y;
                p.SaveConfig();
            }
        }

        // ゲームの言語設定・Mod の言語設定に表示を合わせる（開いている間は毎フレーム呼ばれる）
        public void SyncLanguage(bool force = false)
        {
            if (root == null) return;
            bool changed = L.Resolve(Plugin.Language.Value, GameLanguageCode());
            if (!changed && !force) return;
            foreach (var kv in keyed)
                if (kv.Key != null) kv.Key.text = L.T(kv.Value);
            hintText.text = string.Format(L.T("hint"), Plugin.Instance.ToggleKey.Value);
            langText.text = L.ChoiceLabel(Plugin.Language.Value);
            ApplyFont();
        }

        private static string GameLanguageCode()
        {
            try
            {
                var lm = LocalizationManager.Instance;
                return lm != null ? lm.CurrentLanguage : null;
            }
            catch (Exception) { return null; }
        }

        // ---------------------------------------------------------------- 構築

        private void Build()
        {
            root = new GameObject("SephiriaAoEColor_UI");
            UnityEngine.Object.DontDestroyOnLoad(root);
            root.SetActive(false);

            canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();

            // パネル
            var pgo = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            pgo.transform.SetParent(root.transform, false);
            panel = pgo.GetComponent<RectTransform>();
            panel.anchorMin = panel.anchorMax = new Vector2(0f, 1f);
            panel.pivot = new Vector2(0f, 1f);
            var bg = pgo.GetComponent<Image>();
            bg.color = PanelColor;
            bg.raycastTarget = true;

            // タイトルバー（ドラッグで移動）
            var head = new GameObject("Head", typeof(RectTransform), typeof(Image), typeof(DragHandle));
            head.transform.SetParent(panel, false);
            var hrt = head.GetComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0f, 1f);
            hrt.anchorMax = new Vector2(1f, 1f);
            hrt.pivot = new Vector2(0.5f, 1f);
            hrt.sizeDelta = new Vector2(0f, HeadH);
            head.GetComponent<Image>().color = HeadColor;
            head.GetComponent<DragHandle>().Target = panel;

            var title = Keyed(MakeText(head.transform, "", 13.5f, TextAlignmentOptions.MidlineLeft), "title");
            Stretch(title.rectTransform, new Vector2(10f, 0f), new Vector2(-34f, 0f));
            var close = MakeButton(head.transform, "X", 13f, () => SetOpen(false));
            PlaceRight(close.GetComponent<RectTransform>(), 3f, 24f, 20f);

            float y = -HeadH - 6f;

            // 有効/無効
            var row = NewRow(ref y, RowH);
            Keyed(Label(row, ""), "enable");
            var tgl = MakeButton(row, "", 12f, () =>
            {
                Plugin.Enabled.Value = !Plugin.Enabled.Value;
                Refresh();
            });
            PlaceRight(tgl.GetComponent<RectTransform>(), Pad, 56f, 21f);
            enabledText = tgl.GetComponentInChildren<TMP_Text>();

            // 言語（自動 → 日本語 → English → 한국어）
            row = NewRow(ref y, RowH);
            Keyed(Label(row, ""), "lang");
            var next = MakeButton(row, ">", 12f, () => StepLanguage(1));
            PlaceRight(next.GetComponent<RectTransform>(), Pad, 21f, 21f);
            langText = MakeText(row, "", 12.5f, TextAlignmentOptions.Center);
            PlaceRight(langText.rectTransform, Pad + 23f, 106f, RowH);
            var prev = MakeButton(row, "<", 12f, () => StepLanguage(-1));
            PlaceRight(prev.GetComponent<RectTransform>(), Pad + 131f, 21f, 21f);

            // プレビュー
            y -= 4f;
            row = NewRow(ref y, 18f);
            var pl = Keyed(Label(row, ""), "preview");
            pl.color = SubColor;
            pl.fontSize = 11.5f;
            row = NewRow(ref y, 34f);
            var floor = new GameObject("Floor", typeof(RectTransform), typeof(Image));
            floor.transform.SetParent(row, false);
            Stretch(floor.GetComponent<RectTransform>(), new Vector2(Pad, 2f), new Vector2(-Pad, -2f));
            floor.GetComponent<Image>().color = new Color(0.16f, 0.15f, 0.18f, 1f); // 床っぽい暗色
            floor.GetComponent<Image>().raycastTarget = false;
            var sw = new GameObject("Swatch", typeof(RectTransform), typeof(Image));
            sw.transform.SetParent(floor.transform, false);
            Stretch(sw.GetComponent<RectTransform>(), new Vector2(4f, 4f), new Vector2(-4f, -4f));
            swatch = sw.GetComponent<Image>();
            swatch.raycastTarget = false;
            y -= 4f;

            // RGBA スライダー
            string[] names = { "r", "g", "b", "a" };
            Color[] fills =
            {
                new Color(0.95f, 0.35f, 0.35f, 1f),
                new Color(0.40f, 0.85f, 0.45f, 1f),
                new Color(0.40f, 0.60f, 1.00f, 1f),
                new Color(0.80f, 0.80f, 0.85f, 1f),
            };
            for (int i = 0; i < 4; i++)
            {
                row = NewRow(ref y, RowH);
                Keyed(Label(row, ""), names[i]);
                int ch = i;
                sliders[i] = MakeSlider(row, fills[i], v => OnSlider(ch, v));
                var srt = sliders[i].GetComponent<RectTransform>();
                srt.anchorMin = new Vector2(0f, 0.5f);
                srt.anchorMax = new Vector2(1f, 0.5f);
                srt.pivot = new Vector2(0.5f, 0.5f);
                srt.offsetMin = new Vector2(92f, -9f);
                srt.offsetMax = new Vector2(-50f, 9f);
                sliderValues[i] = MakeText(row, "0", 12.5f, TextAlignmentOptions.MidlineRight);
                PlaceRight(sliderValues[i].rectTransform, Pad, 36f, RowH);
            }

            // カラーコード
            row = NewRow(ref y, RowH);
            Keyed(Label(row, ""), "hex");
            hexField = MakeInput(row, OnHexChanged);
            PlaceRight(hexField.GetComponent<RectTransform>(), Pad, 110f, 21f);

            // ボタン
            y -= 6f;
            row = NewRow(ref y, 26f);
            var reset = MakeButton(row, "", 12.5f, () =>
            {
                Plugin.AoeColor.Value = Plugin.OriginalColor;
                Refresh();
            });
            Keyed(reset.GetComponentInChildren<TMP_Text>(), "reset");
            var rrt = reset.GetComponent<RectTransform>();
            rrt.anchorMin = new Vector2(0f, 0f);
            rrt.anchorMax = new Vector2(0.5f, 1f);
            rrt.offsetMin = new Vector2(Pad, 0f);
            rrt.offsetMax = new Vector2(-3f, 0f);
            var cls = MakeButton(row, "", 12.5f, () => SetOpen(false));
            Keyed(cls.GetComponentInChildren<TMP_Text>(), "close");
            var crt = cls.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.5f, 0f);
            crt.anchorMax = new Vector2(1f, 1f);
            crt.offsetMin = new Vector2(3f, 0f);
            crt.offsetMax = new Vector2(-Pad, 0f);

            // ヒント
            y -= 4f;
            row = NewRow(ref y, 20f);
            var hint = hintText = Label(row, "");
            hint.color = SubColor;
            hint.fontSize = 11.5f;
            y -= 6f;

            panel.sizeDelta = new Vector2(Width, -y);
            panel.anchoredPosition = new Vector2(Plugin.Instance.WindowX.Value, -Plugin.Instance.WindowY.Value);
            DragHandle.ClampIntoParent(panel);
        }

        private TMP_Text Keyed(TMP_Text t, string key)
        {
            keyed.Add(new KeyValuePair<TMP_Text, string>(t, key));
            return t;
        }

        private void StepLanguage(int dir)
        {
            int n = Enum.GetValues(typeof(UiLanguage)).Length;
            int v = ((int)Plugin.Language.Value + dir + n) % n;
            Plugin.Language.Value = (UiLanguage)v;
            SyncLanguage(true);
        }

        // ---------------------------------------------------------------- 値の同期

        private void Refresh()
        {
            suppress = true;
            try
            {
                bool on = Plugin.Enabled.Value;
                enabledText.text = on ? "ON" : "OFF";
                enabledText.color = on ? TextColor : SubColor;

                Color32 c = Plugin.AoeColor.Value;
                swatch.color = c;
                byte[] v = { c.r, c.g, c.b, c.a };
                for (int i = 0; i < 4; i++)
                {
                    sliders[i].SetValueWithoutNotify(v[i]);
                    sliderValues[i].text = v[i].ToString();
                }
                if (!hexField.isFocused) hexField.SetTextWithoutNotify(ToHex(c));
            }
            finally { suppress = false; }
        }

        private void OnSlider(int ch, float value)
        {
            if (suppress) return;
            Color32 c = Plugin.AoeColor.Value;
            byte b = (byte)Mathf.Clamp(Mathf.RoundToInt(value), 0, 255);
            switch (ch)
            {
                case 0: c.r = b; break;
                case 1: c.g = b; break;
                case 2: c.b = b; break;
                default: c.a = b; break;
            }
            Plugin.AoeColor.Value = c;
            Refresh();
        }

        private void OnHexChanged(string s)
        {
            if (suppress) return;
            if (!TryParseHex(s, out Color c)) return;
            Plugin.AoeColor.Value = c;
            suppress = true;
            try
            {
                Color32 c32 = c;
                swatch.color = c;
                byte[] v = { c32.r, c32.g, c32.b, c32.a };
                for (int i = 0; i < 4; i++)
                {
                    sliders[i].SetValueWithoutNotify(v[i]);
                    sliderValues[i].text = v[i].ToString();
                }
            }
            finally { suppress = false; }
        }

        private static string ToHex(Color32 c) => $"{c.r:X2}{c.g:X2}{c.b:X2}{c.a:X2}";

        private static bool TryParseHex(string s, out Color color)
        {
            color = default;
            s = (s ?? "").Trim().TrimStart('#');
            if (s.Length != 6 && s.Length != 8) return false;
            if (!uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint v)) return false;
            if (s.Length == 6) v = (v << 8) | 0xFF;
            color = new Color32((byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v);
            return true;
        }

        // ---------------------------------------------------------------- 部品

        private RectTransform NewRow(ref float y, float h)
        {
            var go = new GameObject("Row", typeof(RectTransform));
            go.transform.SetParent(panel, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, h);
            rt.anchoredPosition = new Vector2(0f, y);
            y -= h;
            return rt;
        }

        private TMP_Text Label(Transform row, string text)
        {
            var t = MakeText(row, text, 13f, TextAlignmentOptions.MidlineLeft);
            Stretch(t.rectTransform, new Vector2(Pad, 0f), new Vector2(-Pad, 0f));
            return t;
        }

        private TMP_Text MakeText(Transform parent, string text, float size, TextAlignmentOptions align)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.fontSize = size;
            t.alignment = align;
            t.raycastTarget = false;
            t.color = TextColor;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.overflowMode = TextOverflowModes.Overflow;
            t.text = text;
            texts.Add(t);
            return t;
        }

        private GameObject MakeButton(Transform parent, string text, float size, Action onClick)
        {
            var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = Color.white; // 実際の濃さは ColorBlock 側で決める（通常 白10%）
            var t = MakeText(go.transform, text, size, TextAlignmentOptions.Center);
            Stretch(t.rectTransform, Vector2.zero, Vector2.zero);
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            var cb = btn.colors;
            cb.normalColor = ButtonColor;
            cb.highlightedColor = new Color(1f, 1f, 1f, 0.22f);
            cb.pressedColor = new Color(0.7f, 0.8f, 1f, 0.32f);
            cb.selectedColor = ButtonColor;
            cb.disabledColor = new Color(1f, 1f, 1f, 0.05f);
            cb.colorMultiplier = 1f;
            btn.colors = cb;
            Tame(btn);
            btn.onClick.AddListener(() =>
            {
                Deselect();
                onClick?.Invoke();
            });
            return go;
        }

        private Slider MakeSlider(Transform parent, Color fillColor, Action<float> onChange)
        {
            var go = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent, false);

            var track = new GameObject("Track", typeof(RectTransform), typeof(Image));
            track.transform.SetParent(go.transform, false);
            var trt = track.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0f, 0.5f);
            trt.anchorMax = new Vector2(1f, 0.5f);
            trt.sizeDelta = new Vector2(0f, 6f);
            track.GetComponent<Image>().color = ButtonColor;

            var fillArea = new GameObject("FillArea", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var fart = fillArea.GetComponent<RectTransform>();
            fart.anchorMin = new Vector2(0f, 0.5f);
            fart.anchorMax = new Vector2(1f, 0.5f);
            fart.sizeDelta = new Vector2(0f, 6f);
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var frt = fill.GetComponent<RectTransform>();
            frt.sizeDelta = Vector2.zero;
            var fimg = fill.GetComponent<Image>();
            fimg.color = fillColor;
            fimg.raycastTarget = false;

            var handleArea = new GameObject("HandleArea", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            Stretch(handleArea.GetComponent<RectTransform>(), new Vector2(4f, 0f), new Vector2(-4f, 0f));
            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            var hrt = handle.GetComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0f, 0f);
            hrt.anchorMax = new Vector2(0f, 1f);
            hrt.sizeDelta = new Vector2(8f, 0f);
            var himg = handle.GetComponent<Image>();
            himg.color = new Color(0.92f, 0.93f, 0.96f, 1f);

            // 行全体でクリックを受ける透明な当たり判定
            var hit = go.AddComponent<Image>();
            hit.color = new Color(0f, 0f, 0f, 0f);

            var s = go.GetComponent<Slider>();
            s.fillRect = frt;
            s.handleRect = hrt;
            s.targetGraphic = himg;
            s.direction = Slider.Direction.LeftToRight;
            s.minValue = 0f;
            s.maxValue = 255f;
            s.wholeNumbers = true;
            var cb = s.colors;
            cb.highlightedColor = new Color(1f, 1f, 1f, 1f);
            cb.pressedColor = new Color(0.7f, 0.8f, 1f, 1f);
            cb.selectedColor = Color.white;
            s.colors = cb;
            Tame(s);
            s.onValueChanged.AddListener(v => onChange(v));
            return s;
        }

        private TMP_InputField MakeInput(Transform parent, Action<string> onChange)
        {
            var go = new GameObject("Input", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = ButtonColor;

            var area = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
            area.transform.SetParent(go.transform, false);
            var art = area.GetComponent<RectTransform>();
            Stretch(art, new Vector2(6f, 1f), new Vector2(-6f, -1f));

            var t = MakeText(area.transform, "", 12.5f, TextAlignmentOptions.MidlineLeft);
            Stretch(t.rectTransform, Vector2.zero, Vector2.zero);

            var input = go.AddComponent<TMP_InputField>();
            input.textViewport = art;
            input.textComponent = t;
            input.targetGraphic = img;
            input.characterLimit = 8;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.contentType = TMP_InputField.ContentType.Alphanumeric;
            input.caretColor = TextColor;
            input.customCaretColor = true;
            input.selectionColor = new Color(0.4f, 0.6f, 1f, 0.4f);
            input.onFocusSelectAll = true;
            Tame(input);
            input.onValueChanged.AddListener(v => onChange(v));
            input.onEndEdit.AddListener(_ => Refresh());
            return input;
        }

        private static void Stretch(RectTransform rt, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        private static void PlaceRight(RectTransform rt, float right, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(-right, 0f);
        }

        // キーボード/パッドのナビゲーションで勝手に選択が移らないようにする
        private static void Tame(Selectable s)
        {
            var nav = s.navigation;
            nav.mode = Navigation.Mode.None;
            s.navigation = nav;
        }

        private static void Deselect()
        {
            try
            {
                var es = EventSystem.current;
                if (es != null) es.SetSelectedGameObject(null);
            }
            catch (Exception) { }
        }

        // ゲームのソフトウェアカーソル（UI_Cursor）より下、他のUIより上に表示する
        private static int SortingOrderBelowCursor()
        {
            try
            {
                var cur = UI_Cursor.Current;
                if (cur != null)
                {
                    var c = cur.GetComponentInParent<Canvas>();
                    if (c != null) return c.rootCanvas.sortingOrder - 1;
                }
            }
            catch (Exception) { }
            return 5000;
        }

        // ---------------------------------------------------------------- ゲームのフォント

        private void ApplyFont()
        {
            if (!TryGetGameFont(out TMP_FontAsset font, out Material mat)) return;
            foreach (var t in texts)
            {
                if (t == null) continue;
                t.font = font;
                if (mat != null) t.fontSharedMaterial = mat;
            }
            if (hexField != null) hexField.fontAsset = font;
        }

        // ゲームの現在の言語のフォントを使う。表示言語の文字が入っていなければ、入っている別のゲームフォントを探す
        private static bool TryGetGameFont(out TMP_FontAsset font, out Material mat)
        {
            font = null;
            mat = null;
            try
            {
                var lm = LocalizationManager.Instance;
                if (lm == null || lm.fontAssets == null || lm.fontAssets.Count == 0) return false;

                string name = lm.GetFontName();
                if (!string.IsNullOrEmpty(name)) lm.fontAssets.TryGetValue(name, out font);
                if (font == null || !Covers(font))
                {
                    foreach (var kv in lm.fontAssets)
                    {
                        if (kv.Value != null && Covers(kv.Value)) { font = kv.Value; name = kv.Key; break; }
                    }
                }
                if (font == null) return false;

                if (lm.fontMaterialPresets != null && name != null &&
                    lm.fontMaterialPresets.TryGetValue(name, out var fm) && fm != null && fm.presets != null)
                {
                    foreach (var p in fm.presets)
                        if (p != null && p.style == "Default") { mat = p.material; break; }
                }
                if (mat == null) mat = font.material;
                return true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("ゲームフォントの取得に失敗: " + e.Message);
                return false;
            }
        }

        private static bool Covers(TMP_FontAsset f)
        {
            try { return f.HasCharacters(L.UsedChars(), out uint[] _, true, true); }
            catch (Exception) { return false; }
        }
    }

    // タイトルバーのドラッグで親パネルを動かす
    internal class DragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        public RectTransform Target;
        private Vector2 grab;

        public void OnBeginDrag(PointerEventData e)
        {
            if (Target == null) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)Target.parent, e.position, e.pressEventCamera, out Vector2 p);
            grab = Target.anchoredPosition - p;
        }

        public void OnDrag(PointerEventData e)
        {
            if (Target == null) return;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    (RectTransform)Target.parent, e.position, e.pressEventCamera, out Vector2 p))
            {
                Target.anchoredPosition = p + grab;
                ClampIntoParent(Target);
            }
        }

        public static void ClampIntoParent(RectTransform rt)
        {
            var parent = rt.parent as RectTransform;
            if (parent == null) return;
            Vector2 size = parent.rect.size;
            if (size.x <= 0f || size.y <= 0f) size = new Vector2(1920f, 1080f);
            Vector2 pos = rt.anchoredPosition;
            // アンカー・ピボットは左上
            pos.x = Mathf.Clamp(pos.x, 0f, Mathf.Max(0f, size.x - rt.sizeDelta.x));
            pos.y = Mathf.Clamp(pos.y, -Mathf.Max(0f, size.y - rt.sizeDelta.y), 0f);
            rt.anchoredPosition = pos;
        }
    }
}
