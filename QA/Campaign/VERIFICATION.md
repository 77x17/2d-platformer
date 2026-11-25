# Cuội Tìm Trăng · Kiểm thử bản Windows 2.0.0

Bản chạy độc lập trên Windows x64, Unity 6000.3.16f1, Direct3D 11 và GeForce RTX 3060. Unity báo dựng bản thành công với 0 lỗi và 0 cảnh báo. [Kết quả build](../build-result.txt)

## Lượt chơi liên tục

Từ trang chủ, trình chơi tự động đưa lệnh chạy, nhảy, leo, lướt và bùng đèn vào cùng bộ điều khiển mà người chơi sử dụng. Va chạm, tim và checkpoint hoạt động trong toàn bộ lượt này. Ba màn đầu kết thúc khi lấy mảnh trăng. Màn bốn tính đến khi Cuội thắp xong sáu mầm và tới đoạn kết.

| Màn | Đồng hồ trong game | Checkpoint | Lần ngã | Lần nhảy giữa không trung | Quãng leo (ô) | Lần lướt |
|---|---:|---:|---:|---:|---:|---:|
| Bờ Tre Làng Cuội | 122,3 s | 6 | 0 | 0 | 0 | 0 |
| Rừng Trúc | 124,0 s | 6 | 0 | 25 | 0 | 0 |
| Sông Đèn | 124,0 s | 6 | 0 | 11 | 31,8 | 0 |
| Đồi Gốc Đa | 124,7 s | 6 | 0 | 13 | 18 | 3 |

Mỗi màn hướng tới khoảng hai phút nội dung. Đây là một lượt tự động quen đường. Thời gian của người chơi còn phụ thuộc vào khám phá, chờ quan sát và số lần thử lại. [Dữ liệu cả chiến dịch](results.json) · [Đoạn kết trong game](route-ending.png)

## Cá chép và quạ

Cả ba cá và ba quạ đều được kiểm trong cảnh đã bố trí. Tâm xuất phát của cá trùng với mặt nước hiển thị. Cành quạ nằm trên đất có chỗ rút lui ở hai phía. Cá báo trước 0,90 giây, quạ báo trước 1,05 giây trong lần đo. Kiểm thử cho Cuội đứng yên chịu đòn, dùng đèn cắt đòn rồi chờ con vật hồi lại, và lùi khỏi vị trí bị nhắm. Mỗi lần trúng mất đúng một tim. Hai lượt né trong clip đều giữ nguyên ba tim. [Cá lao khỏi mặt sông](encounter-fish.png) · [Quạ đang sà](encounter-crow.png)

## Địa hình và hình ảnh

Đã kiểm shader và lưới của mặt đất cho cả bốn màn, từ cạnh ghép kín giữa hai bệ đến vách lộ ra trước vực. Phép dò chân của 233 cụm cảnh dùng 241 tia xuống collider, gồm hai trụ cổng. Trong 12 dải cảnh alpha, cả 1.528 cột của từng dải đều có nền ở chân. Tỉ lệ các điểm ảnh đục nối với phần chân tối thiểu 99,83%. [Dữ liệu tiếp đất](ground-contact.csv) · [Mặt bờ làng](tread-1.png) · [Đá kè ở bến sông](tread-3.png)

Đường chân trời là một bức toàn cảnh duy nhất trong mỗi màn. Trên cùng một quãng camera di chuyển năm ô, bốn lớp lần lượt dịch 0,20 / 1,65 / 3,15 / 5,80 ô. Kiểm từ đầu đến cuối màn và ở nhiều độ cao để phát hiện khe hở. Các ảnh đã quét qua 25 khung, mỗi khung có 0 pixel mang màu tím lỗi shader. [Kết quả quét ảnh](visual-scan.csv)

## Các kiểm tra còn lại

Bộ kiểm thử đầy đủ đạt 72/72 phép kiểm. Bộ kiểm riêng khởi chạy trực tiếp từ thư mục phát hành đạt 69/69. Các tình huống riêng đặt nhân vật đến vị trí cần quan sát để tái hiện nhất quán. Lượt xuyên bốn màn được khởi tạo lại từ đầu và đi bằng điều khiển trong game. Có kiểm menu, pause, nhảy ngắn và dài, nhảy khi vừa rời mép, bấm nhảy trước khi tiếp đất, bè chuyển động, bật đèn, mất tim, hồi checkpoint, mở khóa và kỹ năng theo màn. [Bộ kiểm riêng](distributed-fixtures.json)

Video dài 26 giây, gồm 780 khung hình 960 × 540 lấy từ bản Windows ở 30 fps. Video ghép nhạc của bốn màn và năm hiệu ứng gốc của cá, quạ theo thời điểm báo hiệu và tấn công đã ghi. [Xem gameplay](CuoiTimTrang-gameplay.mp4)

## Tim, chuyển động và piano

Trong Play Mode của Unity Editor, cả 16 ảnh ở bốn kích thước Game View đều cho thấy đúng ba ô tim, số tim đầy tương ứng trạng thái và sprite rỗng cho phần đã mất. Cả bộ đếm lẫn số pixel đỏ của từng ô được đối chiếu với máu thật. [So sánh trái tim](../EditorGameView/hearts-comparison.png) · [Báo cáo riêng của Editor](../EditorGameView/README.md)

Bài kiểm chuyển động trên Windows điều khiển bằng A/D và Space. Cuội chạy rồi dừng và đổi hướng, bám vách, dừng giữa chừng, trèo tiếp và chống tay đặt chân lên đỉnh. Lượt kiểm riêng thu được đủ tám tư thế chạy, tám tư thế leo, bốn tư thế qua mép. [Kết quả chuyển động](motion-results.json) · [Clip cận cảnh 10 giây](Cuoi-chay-va-leo.mp4) · [Tư thế chống tay lên bờ](motion-mantle.png)

Unity Editor nạp đủ sáu bản piano stereo 32 kHz. Trò chơi sử dụng các bản phối từ Trống quân và Đi cấy, cùng hai đoạn thắng/thua và 29 hiệu ứng tiếng động. [Nghe trích đoạn piano](Trung-Thu-piano.mp3) · [Nguồn ký âm và mẫu đàn](../../Documentation/FESTIVAL_PIANO.md)

## Gói phát hành

Đã so mã băm của 181 file runtime trong thư mục chạy với bản build đã kiểm thử, rồi đọc lại và đối chiếu 201 file trong ZIP. Tổng dung lượng 104.410.299 byte. [Tải ZIP Windows](../../Builds/CuoiTimTrang-2.0.0-Windows-x64.zip) · [Mã SHA-256](release.json) · [Nguồn asset](../../CREDITS.md)

