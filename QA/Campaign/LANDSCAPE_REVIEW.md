# Kiểm tra nền cảnh Cuội Tìm Trăng

## So sánh trong bản Windows

Ở bản trước, cây tre, nhà và cổng là những hình rời. Chúng cuộn với tốc độ khác lớp đất, nên khi Cuội tiến ngang hoặc lên cao, người chơi có thể thấy mặt nước hoặc khoảng trời ngay dưới chân nhà.

Trong đoạn chơi 26 giây, bốn màn bắt đầu ở 00:00, 00:04, 00:13 và 00:22. Hai đoạn ở 00:08 và 00:17 ghi lại lần lượt quạ sà và cá chép nhảy. Các ảnh đối chiếu lấy ở 00:03, 00:07, 00:16 và 00:25, gồm cả lúc Cuội đứng trên bờ và đang nhảy. [Xem clip chơi trên Windows](CuoiTimTrang-gameplay.mp4)

Mỗi màn dùng một tranh chân trời liên tục. Camera trượt trong tranh khi Cuội tiến về phía trước. Ba lớp gần hơn cuộn với ba tốc độ riêng, mỗi lớp mang theo phần đất, đá hoặc bờ nước đỡ cảnh vật của chính nó. Nhờ đó đình và cổng giữ một vị trí trên đường chân trời khi camera di chuyển.

| Màn | Chi tiết đã đối chiếu trong hình |
|---|---|
| Bờ Tre Làng Cuội | Nhà và đình đặt trên nền sân, sau hàng rào. Bậc đá, bờ ruộng và đường đi định vị các công trình ở những khoảng cách khác nhau. |
| Rừng Trúc | Dải trúc và dương xỉ nối với nền rừng. Nhà mái tranh tựa trên bờ ruộng bậc thang, phía sau bục có giá đỡ bằng tre. |
| Sông Đèn | Nhà ven sông bám mép bờ kè. Cầu dẫn từ bờ đi qua phần nước giữa hai bến. Ánh phản chiếu nằm bên dưới bến. |
| Đồi Gốc Đa | Tán cây nối với thân và bộ rễ bám sườn. Lớp đá và rễ ở gần vẫn đọc được khi Cuội đứng trước thân cây. |

Mặt đường dùng đất lẫn sỏi ở làng, rễ và rêu ở rừng, đá kè sứt cạnh bên sông, đá phiến lẫn rễ đa trên đồi. Khi Cuội đứng sát mép, có thể thấy phần chân chạm mặt trên và phần vách bắt bóng bên dưới. Các ảnh cắt ở cùng thời điểm trên cùng đường chạy cho thấy thay đổi ở cả bốn màn. [So sánh mặt đường trước và sau](terrain-before-after.jpg)

## Kiểm tra dữ liệu hình ảnh

Chạy `Tools/Test-LandscapeContact.ps1` từ PowerShell để kiểm tra lại các lát ảnh mà camera thực sự dùng. Tệp [ground-contact.csv](ground-contact.csv) chứa đủ 12 phép đo cho bờ xa, bờ gần và mép tiền cảnh ở bốn màn.

* Cả 12 lát ảnh đều có nền đất, đá hoặc nước chạy hết 1.528 cột ở cạnh đáy.
* Với ngưỡng alpha 96/255 và liên thông tám hướng qua đường nối giữa hai tile, 99,8359–100% pixel đạt ngưỡng nối được với đáy của chính lớp đó. Cụm pixel tách lớn nhất trong bốn dải có kiến trúc là 8 pixel nguồn.
* Tại độ cao camera thấp nhất, cạnh cắt dưới của mỗi bờ ở ngoài khung hình ít nhất 0,31 đơn vị Unity. Khi lên cao, camera để lộ thêm phong cảnh phía xa. Hàm `MoonlitCamera.BankGroundClearance` cho phép đo khoảng cách này trong player.

Phép đo alpha kiểm tra tính liền mạch của hình. Việc cửa đình có bậc, nhà có kè và cây có gốc được đối chiếu trực tiếp trên khung hình.
