# Ba tim trong Unity Game View

Trong bản cũ, màn chơi ở độ phân giải 1600 × 900 hiển thị tên màn và thanh hồi lồng đèn, nhưng vùng ba tim để trống. Ảnh dưới lần lượt chụp vị trí đó ở bản cũ, bản sửa khi đầy máu và bản sửa sau một lần trúng đòn. [So sánh trước và sau](hearts-comparison.png)

Bản sửa dùng sprite riêng cho tim đầy và tim rỗng, kèm bộ đếm. Bảng máu bám góc trên bên trái của vùng hiển thị và vẫn xuất hiện lúc xem hướng dẫn hoặc đang hồi sinh.

Đã chạy Play Mode trong **Unity Editor 6000.3.16f1** với Direct3D 11. Phép đo dùng lần vẽ Game View của Editor với Canvas ở chế độ Screen Space Overlay. Mỗi kích thước là một lựa chọn độ phân giải thật trong Game View. Bài kiểm đối chiếu cả `Screen.width`, `Screen.height`, vị trí ba sprite và số pixel đỏ bên trong từng ô.

| Game View | Đầy máu | Hướng dẫn | Sau va chạm | Đang hồi sinh |
|---|---|---|---|---|
| 1600 × 900 | 3/3 | 3/3 | 2/3 | 1/3 |
| 720 × 480 | 3/3 | 3/3 | 2/3 | 1/3 |
| 512 × 288 | 3/3 | 3/3 | 2/3 | 1/3 |
| 480 × 800 | 3/3 | 3/3 | 2/3 | 1/3 |

Ở 512 × 288, ba biểu tượng đầy máu lần lượt có 243, 245 và 247 pixel đỏ trong vùng kiểm. Ô tim rỗng có 0 pixel đỏ. Cả 16 trường hợp đều đạt. [Bảng đo](health.csv) · [Hướng dẫn trong Game View nhỏ](512x288-tutorial.png) · [Ảnh khi mất một tim](1600x900-after-hit.png)

Cũng trong Play Mode, trình kiểm đã quan sát được tám sprite chạy, tám sprite leo, Cuội đặt chân lên bờ sau khi vượt vách, và cả sáu bản piano stereo 32 kHz được Unity nạp thành AudioClip. [Chạy](editor-run.png) · [Leo vách](editor-climb.png) · [Kết quả](motion-and-piano.txt) · [Mã kiểm thử](../../Assets/Moonlit/Editor/GameViewHealthReview.cs)

