# Fugu版 Claude Code 使い方メモ

Sakana AI（Fugu）を「頭脳」として Claude Code を動かすための手順まとめ。
対象プロジェクト: `C:\Users\iniad\Documents\unity\3DpracticeRPG`

---

## 0. 初回だけ：APIキーを登録

PowerShell で一度だけ実行（キーは自分のものに置き換え）。

```powershell
setx SAKANA_API_KEY "fish_あなたのキー"
```

実行後、**PowerShellを閉じて開き直す**と反映される。
確認: `echo $env:SAKANA_API_KEY` でキーが表示されればOK。

---

## 1. 起動方法

### かんたん起動（おすすめ）

プロジェクトフォルダにある `fugu-claude.cmd` を使う。

```powershell
cd C:\Users\iniad\Documents\unity\3DpracticeRPG
.\fugu-claude.cmd
```

`Starting Claude Code (Sakana Fugu)...` と出て Claude Code が立ち上がれば成功。
（フォルダ内の `fugu-claude.cmd` をダブルクリックでも起動可）

### 手動起動（ランチャーを使わない場合）

PowerShell で以下を貼り付けてから `claude` を実行。環境変数はそのウィンドウを閉じるまで有効。

```powershell
$env:ANTHROPIC_BASE_URL="https://api.sakana.ai"
$env:ANTHROPIC_AUTH_TOKEN="fish_あなたのキー"
$env:ANTHROPIC_DEFAULT_OPUS_MODEL="fugu-ultra[1m]"
$env:ANTHROPIC_DEFAULT_SONNET_MODEL="fugu[1m]"
$env:ANTHROPIC_DEFAULT_HAIKU_MODEL="fugu[1m]"
$env:CLAUDE_CODE_SUBAGENT_MODEL="fugu[1m]"
claude
```

---

## 2. 途中から始める（履歴の復元）

`claude` とだけ打つと毎回「新しい会話」になる。続きから始めたいときは引数を付ける。

```powershell
.\fugu-claude.cmd -c      # 直前の会話の続きから再開
.\fugu-claude.cmd -r      # 過去の会話を一覧から選んで再開
```

- `-c` = 直前の続き
- `-r` = 一覧から選択

※ 履歴はPCを閉じても消えない（プロジェクトごとにローカル保存されている）。

---

## 3. モデルの切り替え

Claude Code 内で `/model` を実行して選ぶ。

| 選択肢 | 中身 | 使いどころ |
| --- | --- | --- |
| Default / fugu-ultra[1m] | 最上位・高性能（遅い） | 難しい設計・複雑なバグ |
| fugu[1m] | 標準・バランス型（速い） | 普段のコード作業・軽い相談 |

- 迷ったら **fugu[1m]** が速くて十分。
- **「Fable」は選ばない**（Fuguではなく Anthropic の別モデルに繋がるため）。

---

## 4. スクショで質問する

Fugu は画像認識に対応。Unity画面を見せて質問できる。

1. `Win + Shift + S` で範囲スクショ → フォルダに保存
2. その画像ファイルを Claude Code の入力欄に**ドラッグ＆ドロップ**（一番確実）
   - または `C:\...\スクショ.png このエラーの原因は？` のようにパスを直接入力
3. 画面＋実際のC#コードをまたいだ相談ができる

---

## 5. スキルの確認ダイアログが出たら

`claude-api` などのスキル使用確認が出ることがある（Fuguが `anthropic`/`[1m]` 等の単語で動くため反応する）。
Unityの質問なら **No** でOK。実害はない。

---

## 6. 困ったとき

| 症状 | 対処 |
| --- | --- |
| `SAKANA_API_KEY is not set` | 手順0のキー登録をやり直し、ターミナルを開き直す |
| 文字化けエラーが出る | `.cmd` に日本語を書かない（英語のみにする） |
| 応答が遅い（数分待つ） | `/model` で `fugu[1m]` に切り替える |
| 反応が止まった | `Esc` で中断して入力し直す |

---

## メモ

- Base URL: `https://api.sakana.ai`
- モデル: `fugu`（標準）/ `fugu-ultra`（高性能）
- 用途の使い分け: 会話・調べ物 → Chatbox / Unity実装・ファイル操作 → Claude Code
