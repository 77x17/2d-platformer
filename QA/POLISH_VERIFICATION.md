# Kiểm tra bản polish — 18/11/2026

- Unity 6000.3.16f1, Windows x64, Mono, URP 2D.
- Build cuối: **Succeeded, 0 errors, 0 warnings**; `build-result.txt`, `polish-build.log`.
- SHA-256 của toàn bộ C#, shader và PNG ở project chính khớp nguồn đã build/test.
- **18/18** kiểm tra gameplay cũ đạt: `playtest-results.json`.
- **20/20** kiểm tra polish đạt: `polish-results.json`.
- Ba tiến trình `-moonlitQA`, `-moonlitPolishQA`, `-moonlitRouteQA` đều thoát **mã 0**, dùng `-batchmode -force-d3d11`.
- Route hoàn thành: **59.52 giây, 0 hồi sinh, 37 lần nhảy, 6 lần bừng sáng**, đủ 5 checkpoint, qua puzzle thùng bằng input điều khiển bình thường, không teleport/bất tử: `route-results.json`.

## Lỗi bệ nâng và các kiểm tra quan trọng

- Nhảy từ bệ đang đi lên, rời bệ, đáp trở lại: grounded đúng; animation trở về row 0.
- Đứng qua 2 chu kỳ bệ: **0 frame sai grounded/jump animation**, sai lệch chân với mặt bệ lớn nhất khoảng **0.009 unit** trong lần đo này.
- Nhảy từ bệ đang đi xuống rồi đáp lại đạt.
- Coyote time cấu hình 0.10s: input trong cửa sổ được nhận, sau cửa sổ bị từ chối; bấm lại sau cất cánh không nhảy đôi.
- Bệ sụp tiếp tục rung/rơi/reset sau khi thay ground logic. Controller thông báo `NotifyStanding` từ ground probe, tránh phụ thuộc duy nhất vào `OnCollisionStay2D` khi chân được đặt lại theo mặt bệ mỗi bước.
- Thùng được đẩy **5.50 unit**, đặt lên bàn đạp, cầu hạ và bật collider; Cuội nhảy qua được. Hồi sinh đặt lại thùng/cầu, restart đặt lại cả gió.
- Shader được đưa vào player, hỗ trợ D3D11; bừng sáng mở bán kính từ khoảng 6.4 lên gần 9. Nền cuộn khi đứng yên; gió tăng rồi giảm; cả hai loại quái hiện dấu `!` và hủy khi bị choáng.

## Kiểm tra hình ảnh và cửa sổ

Ảnh `polish-darkness.png`, `polish-light-pulse.png`, `polish-moving-platform.png`, `polish-wind.png`, `polish-dog-warning.png`, `polish-cat-warning.png`, `polish-crate-bridge.png` được render từ camera và UI thực tế. Đã xem trực tiếp các ảnh darkness, pulse/warning/crate và bệ.

Đã mở `Builds/Windows-Polish/Cuoi.exe` qua desktop, quan sát Home, rồi quan sát lượt chơi nhận thao tác người dùng trong cửa sổ thật. Ảnh bằng chứng: `polish-native-window.jpg`. Không đóng lượt đang chơi của người dùng để thử shutdown. Các kiểm tra input tự động dùng controller/Input System; không thay thế playtest cảm giác điều khiển bởi người thật.

Ngoài vùng đèn thế giới bị che đen; HUD vẫn đọc được. Shader này chưa tính occlusion bởi tường. Thời lượng bot không đại diện lần chơi đầu. Log Unity có Curl/TLS cảnh báo môi trường; không có managed exception hoặc shader compilation error trong các lượt test đạt. Ghi chú crash native khi đóng cửa sổ ẩn trên desktop khóa ngày 17/11 chưa được tái kiểm tra, xem `VERIFICATION.md` cho lịch sử.
