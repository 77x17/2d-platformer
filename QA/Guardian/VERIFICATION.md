# Kiểm thử Cây Đa · Windows 2.1.0

Lượt chơi tự động bắt đầu tại điểm xuất phát của Đồi Gốc Đa, đi qua sáu checkpoint, nhặt mảnh trăng thứ tư và về Cung Trăng trong **115,9 giây** theo đồng hồ trong game. Cuội dùng bộ điều khiển chạy, nhảy, lướt và bùng đèn của bản chơi. Kết quả gồm 40 lần nhảy, 13 lần nhảy giữa không trung, 18 ô leo, 7 lần lướt và 0 lần ngã trên cả màn. Thời gian thực tế của người chơi phụ thuộc số lần thử và tốc độ khám phá. [Kết quả gốc](results.json) · [Đoạn kết](route-ending.png)

Riêng trận Cây Đa dài khoảng 40 giây trong lượt này. Cuội còn đủ ba tim suốt trận. Sáu mầm lần lượt nhận ánh sáng ở giây 5,1, 9,7, 15,3, 22,1, 29,1 và 37,9. Trò chơi chuyển sang đoạn kết sau mầm thứ sáu. Có thể xem toàn bộ trận ở tốc độ gốc, với 1.204 khung hình từ camera Unity, 960 × 540, 30 fps. Âm thanh trong clip được dựng lại từ nhạc nền của trận và 111 lần phát hiệu ứng do trò chơi ghi thời điểm. [Video 40 giây](Cuoi-va-Cay-Da.mp4) · [Thời điểm âm thanh](sound-events.csv)

## Những tình huống đã thử

| Tình huống | Kết quả |
|---|---|
| Bùng đèn lúc cây đang lấy đà | Số mầm đã thắp giữ ở 0. |
| Tạm dừng khi cây chuẩn bị xông | Thời gian lấy đà dừng trong suốt khoảng kiểm 0,27 giây. |
| Đứng trong đường xông | Cuội mất đúng một tim, từ ba xuống hai. |
| Bỏ lỡ một lần mầm mở | Cây lấy đà lại và lặp chuỗi. |
| Mất cả ba tim và chọn chơi tiếp | Cuội hồi đầy ba tim ở cột cuối. Cây trở về thế chuẩn bị. |
| Né đòn và bùng đèn bằng điều khiển trong game | Đủ sáu mầm, ba rễ dịu và tới đoạn kết. |

Đã quan sát được cả dấu báo lẫn pha đánh của bốn đòn. Ảnh chụp cho thấy rễ quét cao qua đầu Cuội và hai luồng rễ đất lan khỏi vị trí cây đáp. [Xông ngang](strike-Charge.png) · [Quét thấp](strike-LowSweep.png) · [Quét cao](strike-HighSweep.png) · [Dậm đất](strike-Slam.png) · [Mầm mở](open.png)

## Bản phát hành

Unity 6000.3.16f1 dựng thành công với 0 lỗi và 0 cảnh báo. Bộ kiểm trận boss đạt **8/8**. Từ file ZIP được giải nén riêng, bản Windows chạy **69/69** phép kiểm hồi quy, gồm hiển thị tim, điều khiển, địa hình, âm báo quái và mở kỹ năng. Lượt chơi liên tục của lần sửa này bao phủ toàn bộ màn 4. [Báo cáo build](build-result.txt) · [Kết quả từ ZIP](distributed-fixtures.json)

Đã đối chiếu mã băm của 181 file runtime với bản đã chơi thử, đọc lại 204 mục của ZIP và giải mã cả hai luồng hình, tiếng của MP4. [Bản Windows](../../Builds/CuoiTimTrang-2.1.0-Windows-x64.zip) · [SHA-256](release.json) · [Hình, tiếng và nguồn tham khảo](../../Documentation/BANYAN_DUEL.md)
