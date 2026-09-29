# Sephiria AoE Color

[日本語](#日本語) | [English](#english) | [한국어](#한국어)

A [BepInEx 5](https://github.com/BepInEx/BepInEx) plugin for **Sephiria** that lets you change the color of enemy AoE warnings (the red-pink attack area indicators) from an in-game settings window.

---

## 日本語

セフィリア（Sephiria）の敵 AoE 警告（赤ピンクの攻撃範囲表示）の色を、ゲーム内の設定画面から自由に変更できる BepInEx 5 プラグインです。

### 特徴
- 赤・緑・青・不透明度のスライダー、またはカラーコード（`RRGGBBAA`）で色を指定
- プレビュー付き。「元の色に戻す」でいつでも既定色に戻せます
- ゲームのフォントを使った、ゲーム標準 UI に近い見た目の設定画面
- 日本語 / English / 한국어 に対応。既定ではゲームの言語設定に自動で合わせます
- 表示だけを変える Mod です。ゲームの挙動やマルチプレイの同期には影響しません（他のプレイヤーの画面は変わりません）

### 導入方法
1. [BepInEx 5](https://github.com/BepInEx/BepInEx/releases)（x64、5.4.23 以降）をセフィリアのインストールフォルダに導入し、一度ゲームを起動します。
2. [Releases](../../releases) から `SephiriaAoEColor.dll` をダウンロードします。
3. `Sephiria\BepInEx\plugins\` に `SephiriaAoEColor.dll` を置きます。
4. ゲームを起動します。

### 使い方
- ゲーム中に **F8** で設定画面を開閉します。
- タイトルバーをドラッグすると移動できます。
- 変更した色は**次に出る AoE から**反映されます（表示中の警告は変わりません）。
- 「言語」の `<` `>` で 自動 → 日本語 → English → 한국어 を切り替えます。「自動」はゲームの言語設定に合わせます（日本語・韓国語以外は英語）。

### 設定ファイル
`BepInEx\config\local.sephiria.aoecolor.cfg`（設定画面を閉じたとき・ゲーム終了時に保存）

| 項目 | 既定値 | 内容 |
|---|---|---|
| 有効 | `true` | 色の変更を有効にする |
| AoEの色 | `FF2189E1` | AoE の色（`RRGGBBAA`） |
| 言語 | `Auto` | `Auto` / `Japanese` / `English` / `Korean` |
| 設定画面キー | `F8` | 設定画面を開閉するキー |

### 注意
- ほぼすべての敵・ボス・罠の AoE に反映されますが、一部の攻撃は独自の色を持っているため変わらないことがあります。
- ゲームの「色覚サポート」モードの AoE 色も、この Mod が有効な間は Mod の色で上書きされます。
- カラーコード欄への入力中、キー入力がゲーム側にも届くことがあります。

---

## English

A BepInEx 5 plugin for Sephiria that lets you freely change the color of enemy AoE warnings (the red-pink attack area indicators) from an in-game settings window.

### Features
- Pick a color with Red / Green / Blue / Opacity sliders or a color code (`RRGGBBAA`)
- Live preview, and a "Reset to default" button
- Settings window styled like the game's own UI, using the game's fonts
- Japanese / English / Korean. Follows the game's language setting by default
- Visual only: does not change game behavior or multiplayer sync (other players' screens are not affected)

### Installation
1. Install [BepInEx 5](https://github.com/BepInEx/BepInEx/releases) (x64, 5.4.23 or later) into the Sephiria folder and launch the game once.
2. Download `SephiriaAoEColor.dll` from [Releases](../../releases).
3. Put `SephiriaAoEColor.dll` into `Sephiria\BepInEx\plugins\`.
4. Launch the game.

### Usage
- Press **F8** in game to open/close the settings window.
- Drag the title bar to move the window.
- A new color applies **from the next AoE** (warnings already on screen keep their color).
- Use `<` `>` next to "Language" to cycle Auto → 日本語 → English → 한국어. "Auto" follows the game's language (English for languages other than Japanese/Korean).

### Config file
`BepInEx\config\local.sephiria.aoecolor.cfg` (saved when the window is closed and when the game exits)

| Key | Default | Description |
|---|---|---|
| 有効 (Enabled) | `true` | Enable the color change |
| AoEの色 (AoE color) | `FF2189E1` | AoE color (`RRGGBBAA`) |
| 言語 (Language) | `Auto` | `Auto` / `Japanese` / `English` / `Korean` |
| 設定画面キー (Toggle key) | `F8` | Key that opens/closes the settings window |

### Notes
- Applies to almost every enemy, boss and trap AoE, but a few attacks use their own colors and may not change.
- While enabled, this also overrides the AoE color of the game's color-blind mode.
- While typing in the color code field, key presses may also reach the game.

---

## 한국어

세피리아(Sephiria)의 적 AoE 경고(붉은 분홍색 공격 범위 표시)의 색상을 게임 내 설정 창에서 자유롭게 바꿀 수 있는 BepInEx 5 플러그인입니다.

### 특징
- 빨강 / 초록 / 파랑 / 불투명도 슬라이더 또는 색상 코드(`RRGGBBAA`)로 색상 지정
- 미리보기 제공, "기본 색으로" 버튼으로 언제든 원래 색상으로 복원
- 게임 폰트를 사용한, 게임 기본 UI와 비슷한 모양의 설정 창
- 일본어 / 영어 / 한국어 지원. 기본적으로 게임 언어 설정을 자동으로 따라갑니다
- 표시만 바꾸는 모드입니다. 게임 동작이나 멀티플레이 동기화에는 영향이 없습니다(다른 플레이어 화면은 바뀌지 않습니다)

### 설치 방법
1. [BepInEx 5](https://github.com/BepInEx/BepInEx/releases)(x64, 5.4.23 이상)를 세피리아 설치 폴더에 설치하고 게임을 한 번 실행합니다.
2. [Releases](../../releases)에서 `SephiriaAoEColor.dll`을 다운로드합니다.
3. `Sephiria\BepInEx\plugins\`에 `SephiriaAoEColor.dll`을 넣습니다.
4. 게임을 실행합니다.

### 사용 방법
- 게임 중 **F8**로 설정 창을 열고 닫습니다.
- 제목 표시줄을 드래그하면 창을 옮길 수 있습니다.
- 바꾼 색상은 **다음에 나오는 AoE부터** 적용됩니다(이미 표시 중인 경고는 바뀌지 않습니다).
- "언어" 옆의 `<` `>`로 자동 → 日本語 → English → 한국어를 전환합니다. "자동"은 게임 언어 설정을 따릅니다(일본어·한국어 외에는 영어).

### 설정 파일
`BepInEx\config\local.sephiria.aoecolor.cfg` (설정 창을 닫을 때와 게임 종료 시 저장)

| 항목 | 기본값 | 내용 |
|---|---|---|
| 有効 (사용) | `true` | 색상 변경 사용 |
| AoEの色 (AoE 색상) | `FF2189E1` | AoE 색상(`RRGGBBAA`) |
| 言語 (언어) | `Auto` | `Auto` / `Japanese` / `English` / `Korean` |
| 設定画面キー (설정 창 키) | `F8` | 설정 창을 여닫는 키 |

### 주의
- 거의 모든 적·보스·함정 AoE에 적용되지만, 일부 공격은 고유 색상을 사용하므로 바뀌지 않을 수 있습니다.
- 모드가 켜져 있는 동안에는 게임의 "색약 보조" 모드 AoE 색상도 모드 색상으로 덮어씁니다.
- 색상 코드 칸에 입력하는 동안 키 입력이 게임에도 전달될 수 있습니다.

---

## Building from source

Requirements: .NET SDK (6.0 or later), Sephiria with BepInEx 5 installed.

```
dotnet build SephiriaAoEColor.csproj -c Release -p:GameDir="C:\Path\To\Steam\steamapps\common\Sephiria"
```

The output is `bin/Release/netstandard2.1/SephiriaAoEColor.dll`. Game assemblies are referenced from `GameDir` and are not included in this repository.

### How it works
Almost every enemy attack gets its warning color from the static getter `AOEWarningFactory.HostileWarningColor` (`Color32`) before spawning a `UI_AOEWarning`. This plugin replaces that getter's return value with a Harmony postfix. The settings window is built with uGUI + TextMeshPro, using the game's font assets from `LocalizationManager`.

## Disclaimer
This is an unofficial fan-made mod and is not affiliated with the developers or publishers of Sephiria. Use at your own risk.
