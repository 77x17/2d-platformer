# Cuội Tìm Trăng

Game platformer 2D tiếng Việt trên Windows x64. Cuội mang đèn ông sao qua bốn miền để nhặt lại bốn mảnh trăng và giúp cây đa trở về Cung Trăng. Ngoài chó canh và mèo rình trên cao, Cuội còn gặp cá chép bật khỏi mặt nước và quạ sà xuống từ cành. Có thể đọc dấu hiệu rồi né, hoặc bùng đèn để làm choáng. Ở hồi cuối, Cuội né Cây Đa xông ngang, đọc nhịp quét rễ, nhảy qua sóng đất rồi thắp từng nụ trên thân.

## Chơi bản Windows

Mở `Builds/CuoiTimTrang-Windows/Cuoi.exe` hoặc giải nén trọn bộ `Builds/CuoiTimTrang-2.1.0-Windows-x64.zip` rồi chạy `Cuoi.exe`. Giữ các thư mục dữ liệu và DLL cạnh file thực thi. Từ trang chủ có thể bắt đầu, tiếp tục hoặc chọn lại các màn đã mở. Tiến độ mở màn được lưu trên máy sau khi qua từng màn. Khi thua trong một màn, có thể chơi tiếp ở cột đèn gần nhất.

| Màn | Đặc trưng | Phần thưởng |
|---|---|---|
| 1 · Bờ Tre Làng Cuội | Qua mương trên bó tre, né chó mèo, đẩy thùng lên tường đất và đợi bè. Sau hai cột đèn ở ô 60 và 140, đường làng dẫn qua bờ ruộng, ngõ lò gốm, bến vó rồi trở về sân lễ bên hồ. | Mảnh 1 mở Nhảy đôi |
| 2 · Rừng Trúc | Bật nhảy lần hai để lên tán trúc, theo các gầu nâng ở guồng nước rồi vượt nhịp ván mục lên am cổ. | Mảnh 2 mở Leo tường |
| 3 · Sông Đèn | Leo bờ kè, theo bè qua dòng hoa đăng, đánh thức miếu và thắp ba đèn rước ở bến sông để mở cầu. | Mảnh 3 mở Lướt |
| 4 · Đồi Gốc Đa | Phối hợp ba kỹ năng qua khe gió, thắp cột đèn và vượt sống đá gãy. Sau mảnh cuối, lướt xuyên cú xông, nhảy qua rễ thấp, đứng dưới nhánh cao và tránh hai sóng đất. Khi cây nghỉ, rọi đèn vào mầm vàng. | Mảnh 4 và đoạn kết |

## Điều khiển

| Phím | Hành động |
|---|---|
| A / D hoặc ← / → | Chạy, đẩy thùng khi chạm thùng |
| Space, W hoặc ↑ | Nhảy. Giữ để nhảy cao, nhả sớm để nhảy thấp. Từ màn 2, bấm lần nữa khi trên không. |
| Giữ hướng vào vách và giữ phím nhảy | Leo từ màn 3. Nhả phím nhảy trong khi vẫn giữ hướng vào vách để bám tại chỗ. Giữ nhảy tiếp để trèo qua mép. |
| Shift hoặc K | Lướt từ màn 4 |
| J, X hoặc F | Bùng sáng lồng đèn. Hồi trong 4 giây. |
| E | Thắp miếu ở màn Sông Đèn. Đèn bùng sáng gần miếu cũng mở cầu. |
| Esc · M | Tạm dừng / tiếp tục · bật / tắt âm thanh |
| ↑ / ↓ và Enter, hoặc chuột | Chọn và kích hoạt nút trên menu |

## Project Unity

Mở bằng Unity **6000.3.16f1**, chọn scene `Assets/Moonlit/Scenes/Level01_BanyanForest.unity` rồi nhấn Play. Đây là scene khởi động cho cả bốn màn. `MoonlitGame` quản lý tiến độ, `MoonlitLevel` dựng địa hình từng chương, `CuoiController` xử lý vận động và `BanyanGuardian` điều khiển đoạn gặp Cây Đa. Thông số vận động nằm ở `Assets/Moonlit/Settings/PlayerTuning.asset`.

Camera dùng bốn lớp ngang. Chân trời trượt 0,04 lần quãng đường camera trên một bức toàn cảnh duy nhất của mỗi màn. Cảnh giữa trượt 0,33 lần, bờ gần 0,63 lần và lá tiền cảnh 1,16 lần. Mặt đất và vật thể tương tác đứng trong thế giới ở tỉ lệ 1. Mặt nước phản chiếu dải bờ đang nhìn thấy và những ngọn đèn ở gần. [Nguồn ảnh và prompt](Documentation/campaign-scenery-sources.json)

Cuội có tám tư thế chạy, tám tư thế leo và bốn tư thế chống tay qua mép. Tốc độ đổi chân phụ thuộc vào quãng đường thực đi trên mặt đang đỡ Cuội. Phần piano phối từ **Trống quân** của Bắc Bộ và **Đi cấy** trong tổ khúc múa đèn Đông Anh, Thanh Hóa. [Nguồn sprite chuyển động](Documentation/CUOI_ANIMATION_CREDITS.md) · [Bản ký âm và bản phối](Documentation/FESTIVAL_PIANO.md)

Khi Editor đã đóng, chạy `Tools/Build-Game.ps1` từ PowerShell để build. Nếu Editor đang mở, dùng **Assets → Refresh**, đợi biên dịch xong rồi chọn **Moonlit → 2 - Build Windows executable**. Bản build xuất vào `Builds/CuoiTimTrang-Windows`. Chạy `Tools/Package-Game.ps1` để thêm giấy phép, ghi công và tạo file ZIP phát hành. [Nguồn mỹ thuật, âm thanh và phông chữ](CREDITS.md)

## Kiểm thử

Trận Cây Đa có một lượt kiểm riêng đi từ đầu Đồi Gốc Đa đến đoạn kết và bài tập đứng yên chịu đòn. Dùng `-moonlitGuardianQA` để chạy lại. [Cách đọc các đòn và nguồn sprite](Documentation/BANYAN_DUEL.md) · [Kiểm thử trận mới](QA/Guardian/VERIFICATION.md)

Bản Windows nhận `-moonlitCampaignQA` để chạy kiểm tra tích hợp và một lượt xuyên bốn màn, hoặc `-moonlitRouteQA` để chỉ chạy tuyến chơi liên tục. Báo cáo và ảnh camera được ghi vào `QA/Campaign` của project khi chạy từ đường dẫn build ở trên. Chế độ QA dùng tiến độ độc lập với bản chơi thường. [Kết quả và giới hạn kiểm thử](QA/Campaign/VERIFICATION.md)
