## 詳細
`Rooms By Status` は、ドキュメント内のすべての部屋をステータス別にグループ化して返します。

ステータスには次の状態の部屋が含まれます。
- 適切に配置され境界が設定されている
- 配置解除されている(以前にビューから削除されたが、モデルからは削除されていない)
- 閉じていない(計算された面積がない境界のない部屋)
- 重複した部屋(閉じたスペースを他の部屋と共有する部屋)

次の例では、配置された部屋の面積をクエリーします。
___
## サンプル ファイル

![Rooms By Status](./DSRevitNodesUI.RoomsByStatus_img.jpg)
