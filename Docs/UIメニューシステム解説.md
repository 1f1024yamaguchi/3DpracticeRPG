# スタート画面 UIメニュー / カルーセル システム解説

スタート画面のメニュー生成とカルーセル(ページめくり)表示を実装しているスクリプト群の使い方と、各プログラムの詳細解説です。

## 全体構成

システムは大きく2つのグループに分かれています。

**メニュー生成系** (`Assets/Scripts/UI/`)

| ファイル | 役割 |
|---|---|
| `AutoMenuGenerator.cs` | Inspectorのデータ定義からメニュー項目をEditor上で生成する |
| `GenericMenuItem.cs` | メニュー1項目の実体(Button/Slider/Toggle/Selector/Carousel) |
| `MenuManager.cs` | メニュー階層の開閉・アニメーション・フォーカス管理 |
| `Editor/AutoMenuGeneratorEditor.cs` | Inspectorに「メニューを生成/クリア」ボタンを追加 |
| `Editor/GenericMenuItemEditor.cs` | ItemTypeに応じて必要な項目だけInspectorに表示 |
| `Editor/MenuEntryDataDrawer.cs` | menuItemsリスト内の各要素の描画を定義 |

**カルーセル系** (`Assets/3DRPG/MY_UI/MultiMedia/`)

| ファイル | 役割 |
|---|---|
| `MediaPageData.cs` | 1ページ分のデータ(説明文+画像or動画) |
| `CarouselMenuController.cs` | メニュー内カルーセルのページ送り操作・インジケータ |
| `MultiMediaPresenter.cs` | テキスト・画像・動画の表示専門クラス |
| `MultiMediaContent.cs` | ギャラリーシーン用の項目(項目自身がページを持つ) |
| `MultiMediaGenerator.cs` | ギャラリーシーン用の項目自動生成 |
| `Editor/MultiMediaGeneratorEditor.cs` | Inspectorに「Generate/Clear Menu」ボタンを追加 |

### データと処理の流れ

```
[Editor作業時]
AutoMenuGenerator.menuItems (Inspectorで定義)
        │ 「メニューを生成」ボタン
        ▼
GenericMenuItem を Prefab から複製 → 値を流し込み → ナビゲーションリンク設定 → シーンに保存

[実行時]
MenuManager ──(開閉/スタック管理)──> 各メニュー(GameObject)
GenericMenuItem ──選択──> MenuManager.UpdateDescription / UpdatePreviewVideo / ShowPreview
GenericMenuItem(Carousel) ──決定──> CarouselMenuController.Initialize
CarouselMenuController ──表示依頼──> MultiMediaPresenter.DisplayPage
```

---

## 使い方

### 1. メニューを作る (AutoMenuGenerator)

1. メニューの親となる GameObject(パネル等)に `AutoMenuGenerator` をアタッチする
2. Inspector で以下を設定する
   - **Menu Item Prefab**: `GenericMenuItem` が付いた雛形Prefab
   - **Container**: 生成した項目を並べる親(VerticalLayoutGroup推奨)
   - **Menu Items**: 項目を上から順に定義(下記参照)
   - **Allow Cancel**: このメニューでキャンセル(戻る)操作を許可するか
3. Inspector下部の **「メニューを生成」** ボタンを押す
4. **シーンを保存する**(生成結果はシーンに焼き付けられるため、Runtimeでの生成処理は不要)

やり直す時は「クリア」→ 定義を修正 → 再度「メニューを生成」。

### 2. Menu Items の各項目の設定

| フィールド | 内容 |
|---|---|
| Item Name | 項目名(表示テキスト) |
| Type | Button / Slider / Toggle / Selector / Carousel |
| Description | 選択中に説明欄へ表示する文章 |
| Command Input Text | 操作説明(例: ↓↘→ + P) |
| Preview Video | 選択中に再生する動画 |
| Target Sub Menu | 決定で開くサブメニュー(Carouselの場合はCarouselMenuControllerを持つオブジェクト) |
| Player Prefs Key | 設定すると値を保存/復元するキー(※現在は連携コードがコメントアウト中) |
| Is Permitted | falseにすると選択はできるが決定できない項目になる |
| Initial/Min/Max Value | Slider用の初期値・範囲 |
| Selector Options | Selector用の選択肢文字列 |
| Media Pages | Carousel用のページデータ |
| OnSubmit / OnValueChanged | 決定時・値変更時に呼ぶイベント |

Typeを変えると、Inspectorにはその種類に必要な項目だけが表示されます(`MenuEntryDataDrawer` の機能)。

### 3. メニュー階層をつなぐ (MenuManager)

1. Canvas配下に `MenuManager` をアタッチしたオブジェクトを置く
2. **First Menu** に最初に開くメニューを設定(Startで自動的に開く)
3. **Description Text / Preview Video Player / Preview Video UI** に表示先を割り当てる
4. **Transition Type** でメニュー切替アニメーションを選択
   - `Slide`: 左右スライド(既定) / `Fade`: フェード / `Instant`: 即時 / `Cascade`: 前のメニューを残して重ねる
5. サブメニューは各項目の Target Sub Menu に設定するだけで、開く(決定)/戻る(Escape・パッドB)が自動で機能する

シーン遷移は項目の OnSubmit イベントに `AutoMenuGenerator.CallFadeToScene(シーン名)` を登録します。

### 4. メニュー内カルーセルを作る (Carousel項目)

1. カルーセル表示用のサブメニュー(パネル)を作り、`CarouselMenuController` をアタッチ
2. 同じパネル内に `MultiMediaPresenter` を置き、表示先UIを割り当てる
   - Item Name Text / Description Text (TextMeshProUGUI)
   - Display Image (Image) / Video Player + Video Render Texture (RawImage)
3. `CarouselMenuController` の設定
   - **Presenter**: 上記の MultiMediaPresenter
   - **Horizontal Layout Group Obj**: ページインジケータ(●○)を並べる親
   - **Active/Inactive Sprite・Color**: インジケータの見た目
4. AutoMenuGenerator側で項目の Type を **Carousel** にし、
   - **Target Sub Menu** = このカルーセルパネル
   - **Media Pages** = 各ページの説明文+画像/動画
5. 「メニューを生成」→ シーン保存

実行時の操作: 項目を選択するだけで1ページ目がプレビュー表示され、決定でカルーセルモードに入り、左右キーでページ送り(ループ)、キャンセルで元のメニューに戻ります。

### 5. ギャラリー専用シーンを作る (MultiMediaGenerator)

MenuManagerを使わない単独シーン(実績・ムービーギャラリー等)向けです。

1. `MultiMediaGenerator` をアタッチし、Item Prefab(`MultiMediaContent`付き)・Container・Presenter を設定
2. **Items** に「項目名+ページリスト」を定義
3. Inspectorの **「Generate Menu」** ボタンで生成 → シーン保存
4. 実行時は上下で項目移動、左右でページ送り、Bボタンで前のシーンへ戻る(`cancelTargetScene` がフォールバック先)

---

## 各プログラムの詳細解説

### AutoMenuGenerator.cs

メニューを「Editorで手動生成」するコンポーネント。Runtime生成を行わないのが設計上のポイントで、生成結果をシーンに焼き付けることで実行時の初期化コストと不確定要素を無くしています。

- `MenuEntryData` (Serializableクラス): 1項目分の定義データ。GenericMenuItemが持つほぼ全フィールドの「入力元」
- `GenerateMenu()`: 既存項目をクリア → menuItemsを順に走査し、Prefabを複製して各値を流し込み → `RefreshUI()`で表示反映 → `LinkNavigation()`
- `LinkNavigation()`: `Navigation.Mode.Explicit` で上下リンクを設定。`(i - 1 + count) % count` の剰余計算により先頭↔末尾がループする
- `_generatedItems`: `[HideInInspector, SerializeField]` にすることで、Inspectorを汚さずにシーンへシリアライズ(=クリア時に確実に破棄できる)
- `ClearMenu()`: Playモードでは `Destroy`、Editorモードでは `DestroyImmediate` を使い分け
- `CallFadeToScene()` / `RestartThisScene()`: InspectorのUnityEventから呼ぶためのシーン遷移ヘルパー

### GenericMenuItem.cs

メニュー1項目の実体。`Selectable` 継承なのでEventSystemの選択・決定・クリックがそのまま使え、`ItemType` で5種類の挙動を1クラスで実現しています。

- **入力処理の二重構造**: 決定/クリックは `ISubmitHandler`/`IPointerClickHandler` のイベント、Slider/Selectorの左右はイベントではなく `Update()` でのポーリング。ポーリングにしているのは「押しっぱなしで連続変更」を `inputCooldown`(0.2秒間隔)付きで実現するため
- `OnSelect()`: 説明文・プレビュー動画・サブメニュープレビューをMenuManagerへ反映。Carouselなら1ページ目を先行プレビュー
- `OnDeselect()`: 表示をクリア。ただし `_isRetainedFocus`(サブメニューに潜った状態)ならカルーセル表示を残す
- `OnSubmit()`: `isPermitted` チェック → Toggleなら値反転、CarouselならInitialize → `OnSubmitEvent` 発火 → サブメニューがあれば `MenuManager.OpenMenu`
- `SetRetainedFocus()`: サブメニューを開いた時、親メニュー側の項目を「選択されたままの見た目」で残すための仕組み。MenuManagerから呼ばれる
- `RefreshUI()`: タイプ別に値テキストを更新(Slider=数値、Toggle=ON/OFF、Selector=選択肢文字列)
- `TryAutoFetchReferences()`: TMPro参照が未設定なら子から自動取得(1つ目=ラベル、2つ目=値)
- `GetHorizontalInput()`: 新InputSystemでキーボード(←→/A/D)とゲームパッド(十字キー→左スティックの順)を統合

### MenuManager.cs

メニュー階層全体の司令塔。核となるのは `Stack<MenuState>` によるスタック管理で、「開く=Push、戻る=Pop」というシンプルなモデルです。

- `MenuState`: メニュー本体と「開く直前に選択していた項目」のペア。これにより戻った時にフォーカスが元の位置へ復元される
- `OpenMenu()`: 現在のメニューを左へ退場+操作無効化(`SetFocusEffect`)+フォーカス保持表示(`MarkRetainedFocus`) → 新メニューをPushして右から入場 → `FocusFirst()` で先頭項目にフォーカス
- `CloseMenu()`: Popして右へ退場 → 親メニューを左から入場 → `RestoreFocus()`
- **アニメーション**: `TransitionCoroutine` がFade/Slideの実体。`Time.unscaledDeltaTime` 使用(ポーズ中でも動く)、イーズアウト補間 `1-(1-t)³`、`_activeTransitions` 辞書で同一メニューのコルーチン重複を防止
- **プレビュー機能**: `ShowPreview()` は「操作できない見せるだけ」の状態でサブメニューを表示。`CanvasGroup.interactable/blocksRaycasts` をオフにして実現。閉じるアニメーション中なら強制停止してから表示
- `MaintainSelection()`: マウスクリック等でEventSystemの選択がnullになった時、最後の選択項目へ自動復帰(キーボード操作不能になるのを防ぐ保険)
- `HandleCancelInput()`: Escape/パッド東ボタンで `CloseMenu()`。ただしメニューの `AutoMenuGenerator.allowCancel` がfalseなら無視
- `CanPlayNavigationSound`: 遷移直後の自動フォーカスで効果音が鳴る誤動作を防ぐフラグ。遷移開始でfalse→1フレーム後にtrue

### MediaPageData.cs

カルーセル1ページ分のデータ定義。`mediaType` により `imageSprite`(静止画) / `videoClip`(動画) / `videoUrl`(URL動画)を使い分けます。Serializableなので、AutoMenuGeneratorの `mediaPages` やMultiMediaGeneratorの `pages` にInspectorから直接記入できます。

### CarouselMenuController.cs

メニュー内カルーセルの操作担当。`Selectable` 継承なのでMenuManagerのサブメニューとして開いた時に自動でフォーカスされます。

- `OnMove()`: 上下入力は `eventData.Use()` で消費して無視(フォーカスが他へ飛ばないように)。左右で `ChangePage(±1)`
- `ChangePage()`: ページ番号を増減し、端でループ。その後Presenterとインジケータを更新
- `ShowPreview()` / `ClearPreview()`: 決定前のプレビュー用。GenericMenuItemのOnSelect/OnDeselectから呼ばれる
- `GeneratePageIndicators()`: ページ数分の●をコードで動的生成(`new GameObject` + `Image`)。HorizontalLayoutGroupが整列を担当
- 表示処理は一切持たず、すべて `MultiMediaPresenter` に委譲

### MultiMediaPresenter.cs

表示専門クラス(Presenterパターン)。入力は扱いません。

- `DisplayPage()`: タイトル・説明文を更新し、`UpdateMediaDisplay()` でメディアを切替
- `UpdateMediaDisplay()`: まず画像・動画を全部非表示/停止してから必要なものだけ表示。動画は `Play()` ではなく `Prepare()` → `prepareCompleted` コールバック → `Play()` の順にすることで、**ロード完了までRawImageを隠して前の動画の残像が見えるのを防ぐ**のがポイント
- `OnVideoPrepared()`: イベントを解除してからRawImage表示+`FadeVideoIn()` でフェードイン再生
- イベントの多重登録防止のため、切替時に必ず `prepareCompleted -= OnVideoPrepared` を実行

### MultiMediaContent.cs

ギャラリーシーン用の項目。CarouselMenuControllerと似ていますが、**項目自身がページデータを保持**し、選択しただけで左右ページ送りができます(決定操作が不要)。

- `Initialize()`: Generatorからデータを受け取る入り口。ラベルテキストは未設定なら子から自動取得
- `OnMove()`: 上下は `base.OnMove()` に任せて隣の項目へ移動、左右はページ送り
- `OnSelect()`: 選択されるたびにページを1ページ目へリセットし、Presenterへ表示
- `OnIndicatorUpdate`: `Action<int,int>`(ページ総数, 現在index)。Generator側の `UpdateIndicator` が登録され、ページが変わるたび通知される
- `_presenter` を `[SerializeField, HideInInspector]` にすることで、Editor生成した参照がPlayモードでも保持される

### MultiMediaGenerator.cs

ギャラリーシーン用の生成器。AutoMenuGeneratorのMultiMedia版です。

- `GenerateMenu()`: 項目生成+ナビゲーションリンク+**全項目中の最大ページ数分**のインジケータを一括生成。項目ごとのページ数の差は `UpdateIndicator()` の表示/非表示で吸収する設計
- `Awake()`: シーンに焼き付けられた項目のイベント(`OnIndicatorUpdate`)はシリアライズされないため、Play開始時に再登録する
- `OnEnable()` → `FocusFirstItemCoroutine()`: 1フレーム待ってから先頭項目にフォーカス(他のUI初期化との競合回避)
- `Update()`: Bボタンで `AppSessionManager.previousSceneName`(なければ `cancelTargetScene`)へフェード遷移
- `ClearMenu()`: 生成物に加え、Editor時はコンテナ配下・インジケータ親配下も掃除

### Editor拡張 3ファイル

- **AutoMenuGeneratorEditor**: Inspector末尾に「メニューを生成/クリア」ボタンを描画。実行後 `MarkSceneDirty` でシーンを未保存状態にし、保存忘れを防ぐ
- **GenericMenuItemEditor**: `SelectableEditor` を継承し、標準のInteractable/Navigation等の下に独自プロパティを描画。`itemType` の値を見て、Sliderならmin/max、Selectorならoptions、Carouselならmediapages…と必要な項目だけを表示
- **MenuEntryDataDrawer**: `MenuEntryData` 用のPropertyDrawer。Foldout付きで、`GetPropertyHeight()` で展開状態・タイプに応じた高さを正確に計算(これを誤るとリスト表示が崩れる)

---

## よくあるハマりどころ

- **生成後にシーン保存を忘れる** → Editor生成方式のため、保存しないと生成結果が消えます
- **項目を変更したのに反映されない** → menuItemsの編集後は再度「メニューを生成」が必要です
- **カルーセルでページ送りできない** → Media Pagesが2ページ以上あるか、Target Sub MenuにCarouselMenuControllerが付いているか確認
- **動画が表示されない** → MultiMediaPresenterのVideo PlayerとRawImage(Render Texture)の両方の割り当てが必要です
- **PlayerPrefsKeyを設定しても保存されない** → SystemSettingsManager連携コードが現在コメントアウトされています(GenericMenuItem.LoadSettings / SaveSettings)
