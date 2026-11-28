# Cuội Tìm Trăng 2.4.0

Bản Windows x64 trên Unity 6000.3.16f1. Đã chạy bản dựng qua bốn màn, đến trận Cây Đa và đoạn kết. [Bản phát hành](../../Builds/CuoiTimTrang-2.4.0-Windows-x64.zip) · [Ảnh trong game](preview.jpg)

## Hành trình

| Màn | Thời gian trong game | Hạt trăng | Cột đèn |
|---|---|---|---|
| Bờ Tre Làng Cuội | 2 phút 17 giây | 10/10 | 6/6 |
| Rừng Trúc | 2 phút 28 giây | 10/10 | 6/6 |
| Sông Đèn | 2 phút 10 giây | 10/10 | 6/6 |
| Đồi Gốc Đa | 1 phút 58 giây | 10/10 | 6/6 |

Lượt đi liên tục dùng bộ điều khiển của Cuội để đi bộ, nhảy, lướt, leo vách và đi bè. Thời gian trên dành cho đường đi đã biết. Người chơi tự khám phá có thể cần thêm thời gian để tìm hạt và quay lại.

## Những tình huống đã thử

- Ở Rừng Trúc, đi theo cầu dưới cho đến cổng khóa, quay lại nhảy lên giàn cao, đạp hai chuông và trở về đường dưới. Lượt thử riêng đã đạp chuông phía đông trước, đạp lại cùng một bàn rồi sang bàn phía tây. Một chuông không mở được cổng.
- Mỗi scene lưu mười vị trí hạt trong nhóm `Hidden Moon Motes`. Đã chạm mảnh lớn trước khi gom đủ hạt ở cả bốn màn, kiểm tra mũi tên hai phía, thử quay lại tìm và kiểm lại lúc có chín hạt. Khi đủ mười, mũi tên tắt và Cuội lấy được mảnh lớn. [Mảnh trăng báo thiếu hạt](fragment-needs-motes.png)
- Đã kiểm tra màn hình tải có tranh, tên nơi đến và thanh tiến độ khi rời Home. [Ảnh màn hình tải](loading-village.png)
- Đã chạy bè từ bến trái rồi cho Cuội đứng yên trên chuyến về. Đã thực hiện chuỗi leo, dừng, leo tiếp và trèo qua mép bằng bàn phím trong bản Windows.

| Tình huống | Số đo từ bản chạy |
|---|---|
| Bè đi sang phải | samples=253 unstable=0 horizontal drift=0.000u vertical gap=0.000u |
| Bè quay về trái | samples=271 airborne=0 running=0 max visual gap=0.000u horizontal drift=0.000u |
| Giữ người khi đổi tư thế leo | 16 sprite changes, largest relative lantern shift=0.028u |
| Dừng bám và leo tiếp | 18 held samples, body drift=0.000u, phase drift=0.000, resumed=True |
| Bàn tay bám vách | 33 hands on wall; max horizontal gap=0.000u, vertical grip slide=0.006u |

Các bộ kiểm tra có 35 mục về giao diện, cảnh và hạt trăng, 14 mục về các câu đố, cùng 71 mục trong lượt chạy toàn chiến dịch. [Kết quả giao diện và hạt](finalpass.json) · [Kết quả di chuyển](crossings.json) · [Kết quả chiến dịch](campaign.json) · [Thông tin bản đóng gói](release.json)

Bản game giải nén từ ZIP cũng đã chạy các phép thử giao diện và hạt trăng đến đoạn kết. Lượt thử trận Cây Đa riêng hoàn thành cả sáu nụ. [Bản ZIP đã chạy](packaged.json) · [Trận cuối](guardian.json) · [Kiểm tra chín ảnh từ camera](visual-scan.csv)
