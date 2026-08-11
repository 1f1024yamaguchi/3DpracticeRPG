# オートセーブ機能 セットアップガイド（Pogostuck方式）

作成日：2026年7月14日

## 変更ファイル一覧

| ファイル | 内容 |
|---|---|
| `Assets/3DRPG/Scripts/Managers/AutoSaveManager.cs` | 新規。オートセーブ本体 |
| `Assets/3DRPG/Scripts/TimerController.cs` | `CurrentTime` プロパティと `SetTime()` を追加 |
| `Assets/3DRPG/Scripts/PlayerStatus.cs` | `RestoreDamagePercent()` を追加 |

保存内容：座標・向き（Y回転）・シーン名・経過時間・ダメージ％（スマブラ形式）。
保存先は PlayerPrefs（キー `TOPROAD_AUTOSAVE_V1`）。Steam（Windows）ではレジストリ、unityroom（WebGL）ではIndexedDBにUnityが自動で振り分けるため、プラットフォーム分岐なしで動く。

## セットアップ手順

1. Unityでプロジェクトを開き、コンパイルエラーが無いことをConsoleで確認する。
2. **Main シーン**を開く。
3. 空のGameObjectを作成し、名前を `AutoSaveManager` にして `AutoSaveManager` コンポーネントをアタッチする。
4. Inspectorの参照（Player / Timer Controller）は**未設定でOK**。実行時にシーンから自動検索する。明示的に挿しても良い。
5. 保存設定はデフォルト（保存間隔0.5秒 / 距離しきい値2m / Flush間隔5秒）のままで動く。

タイトル画面の対応（推奨）：

- 「はじめから」ボタンの処理で `AutoSaveManager.DeleteSave()` を呼ぶ。これをしないと、はじめからを選んでも前回位置から再開してしまう。
- 「つづきから」の表示判定には `AutoSaveManager.HasSave()` が使える。
- ゲームクリア時（リザルトへ遷移する時）にも `AutoSaveManager.DeleteSave()` を呼ぶと、クリア済みデータが山頂で残り続けない。

## 動作確認チェックリスト

1. **保存**：Mainシーンで少し移動 → Playを停止 → 再Play → 移動後の位置・向きで再開すること。Consoleに `[AutoSave] ロード完了: ...` が出る。
2. **タイマー復元**：再開時にタイマーが続きの値から進むこと（0に戻らない）。
3. **ダメージ％復元**：敵に殴られて％を上げた状態で再Play → ％がUIに復元されること。
4. **落下時**：高所から落下 → そのまま数秒待つ → 再Play → **落ちた後の位置**から再開すること（セーブが位置を戻さない＝仕様通り）。
5. **死亡時**：死亡（Die状態）中はセーブされない。死亡直前の位置・％が残ることを確認。
6. **初回起動**：PlayerPrefsを消した状態（メニュー Edit > Clear All PlayerPrefs でも可。ただしレベル・アイテムデータも消える点に注意）で、シーン初期位置から始まること。
7. **WebGLビルド**：unityroomかローカルWebGLビルドで、タブを閉じて開き直しても位置が復元されること。WebGLはOnApplicationQuitが呼ばれないことがあるため、フォーカスが外れた時点で強制保存する実装にしてある。タブを「即クローズ」した場合のみ最大5秒（Flush間隔）分の巻き戻りが起きうるが、仕様上許容範囲。

## 設計上の注意点

- **落下時にセーブ機構は何もしない**。位置を戻す処理・無効化する処理は入れていない。落下後の位置が次の保存周期（0.5秒）で上書きされるだけ。ここに「落下したら保存を止める」等の処理を将来追加しないこと（壺らしさが消える）。
- **テレポート時のCharacterController**：有効なままtransformを書き換えると内部位置とズレて元の位置に引き戻されるため、AutoSaveManagerは 無効化→座標設定→再有効化 の順で処理している。リスポーン処理などを別途書く場合も同じ手順を守ること。
- **空中で終了した場合**は空中の座標が保存され、再開時にそこから落下する。仕様通りだが、気になる場合はAutoSaveManagerのUpdate内の保存条件に `_characterController.isGrounded` を足せば接地時のみ保存に変えられる（1行）。
- **シーン名チェック**：保存データのシーン名が現在のシーンと違う場合は適用しない。Mainシーンの名前を変えたら旧セーブは無効になる。データ構造を変更する時は `SaveKey` を `_V2` に上げて旧データと縁を切る。
- **Flush間引き**：PlayerPrefs.SetStringは毎回（0.5秒毎）行うが、ディスク確定のPlayerPrefs.Save()は5秒毎に間引いている。WebGLでのIndexedDB同期によるヒッチ防止のため。デスクトップではアプリ終了時にもUnityが自動保存する。

## 既知の制限（今回のスコープ外）

- レベル・経験値は `LevelSystem`、所持アイテムは `OwnedItemsData` が既に個別にPlayerPrefs保存している。オートセーブとは保存タイミングが別なので、厳密に同期させたい場合は将来AutoSaveManagerに統合する。
- 攻撃力/速度/ジャンプ力のスキル強化分（`_originalAttackPower` 等）は保存されない。
- 倒した敵・ボス進行フラグは保存されない（ボス実装時に `SaveData` へ追加すること）。
- スタートシーンの「はじめから/つづきから」分岐は未実装（上記の推奨対応を参照）。
