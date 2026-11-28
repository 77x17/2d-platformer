# Kiểm thử Cuội Tìm Trăng 2.3.0

Bản Windows x64 được dựng bằng Unity 6000.3.16f1 ngày 28/11/2026. [Kết quả build](build-result.txt) · [Tệp ZIP đã kiểm tra](../../Builds/CuoiTimTrang-2.3.0-Windows-x64.zip)

## Từ menu đến đoạn kết

Trong Unity, mở `00_Home` và nhấn Play để vào menu. Chọn **Bắt đầu** sẽ mở cảnh trăng vỡ rồi đưa Cuội về làng. Mở trực tiếp scene của bất kỳ màn nào rồi nhấn Play để thử riêng màn đó. Khi chạy bản Windows, Home luôn là cảnh đầu tiên.

| Lượt kiểm tra | Kết quả |
|---|---|
| Mở từng scene trong Unity Editor | 5/5 đạt. Menu có tranh nền riêng. Cuội xuất hiện và di chuyển được ở cả bốn màn. Mỗi lần chỉ có một camera, một bộ nghe âm thanh và một bộ điều khiển giao diện. Tiến độ lưu trên máy được giữ nguyên. [Kết quả Editor](editor-direct-entry.json) |
| Menu, chuyển màn, môi trường và đoạn kết | 28/28 đạt. Qua bốn scene, nhặt đủ bốn mảnh, thắp sáu nụ, quay về Home rồi mở lại Rừng Trúc từ bảng chọn màn. Mục **Bản quyền** mở được ở cả đầu và cuối game. Hai bảng hiển thị đủ 65 ký tự tiếng Việt có dấu được kiểm tra. [Kết quả](results.json) |
| Điều khiển, hình ảnh và hành trình liên tục | 70/70 đạt. Đã kiểm tra ba tim trên HUD, chạy, bám vách, trèo qua mép, quái xuất hiện ở đúng môi trường và chơi một lượt tới đoạn kết. [Kết quả hành trình](campaign-results.json) |
| Chạy trực tiếp từ bản ZIP đã giải nén | 11/11 đạt. Kiểm tra cảnh mở, hồi sinh khi rơi xuống chông và giải câu đố ở từng màn bằng bộ điều khiển của Cuội. [Kết quả bản phát hành](crossing-results.json) |

## Nhịp của bốn màn

Lượt thử tự động dùng cùng bộ điều khiển và va chạm với bàn phím. Thời gian dưới đây lấy từ đồng hồ từng màn. Người chơi có thể dừng lại nhặt hạt trăng, xem cảnh hoặc quay lại cột đèn sau khi ngã.

| Màn | Thời gian | Cột đèn đi qua | Lần ngã |
|---|---|---|---|
| 1 · Bờ Tre Làng Cuội | 2 phút 06 giây | 6/6 | 0 |
| 2 · Rừng Trúc | 2 phút 05 giây | 6/6 | 0 |
| 3 · Sông Đèn | 2 phút 05 giây | 6/6 | 0 |
| 4 · Đồi Gốc Đa | 1 phút 59 giây | 6/6 | 0 |

## Hình ảnh và âm thanh

Đã đối chiếu 277 điểm tiếp xúc giữa phần ảnh của cây, cọc, kiến trúc và mặt đất trên bốn màn. Hai mươi chỗ bắt dây trên các bệ nâng đã được kiểm tra. Ở Rừng Trúc, bệ di chuyển 0,33 đơn vị trong lần lấy mẫu và cả hai dây vẫn nối từ xà xuống mặt ván. [Gốc đa ở làng](village-root-contact.png) · [Bệ nâng ở rừng](forest-lift-attachments.png)

Cổng trúc mở sau khi Cuội đáp lên hai phiến đá. Trên đồi, chiếc cầu có đối trọng hạ xuống mố. Có thể xem cả hai trong [ảnh đối chiếu môi trường](environment-review.jpg). Bản Windows phát lớp tiếng rừng và sông bằng âm thanh stereo tại đúng màn. [Nguồn âm thanh và mỹ thuật](../../CREDITS.md)

Đã xem ảnh chụp từ camera trong game và quét 15 ảnh PNG để tìm màu hồng báo lỗi shader của Unity. Bộ quét ghi nhận 0 điểm ảnh lỗi. [Kết quả quét](visual-scan.csv) · [Menu](home-title.png) · [Chọn màn](home-chapter-selection.png) · [Đoạn kết](route-ending.png)

Đã đối chiếu SHA-256 của 216 tệp trong ZIP với bản dựng và tư liệu nguồn. [Thông tin bản phát hành](release.json)
