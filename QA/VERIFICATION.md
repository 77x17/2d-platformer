# Kiểm tra bản bàn giao — 17/11/2026

- Unity 6000.3.16f1, Windows x64, Mono, URP 2D.
- Build cuối: thành công, **0 errors / 0 warnings**. `build-result.txt`.
- Source C# ở project chính đã so SHA-256 với source dùng build kiểm tra: khớp toàn bộ.
- **18/18 integration checks đạt**. Có phím Enter/D/Escape qua Input System, spawn, gia tốc, jump height, coyote, jump buffer, lantern stun/cooldown/recovery, va chạm, checkpoint, rơi vực, bệ động, bệ sụp, mở cầu, pause, nhận trăng, restart. `playtest-results.json`.
- Lượt đi liên tục trên màn mở rộng: **hoàn thành trong 65,99 giây**, 41 lần nhảy, 7 lần bừng sáng, 1 lần hồi sinh, đi qua 5 checkpoint. Bot dùng controller và va chạm thật; không teleport/cheat bất tử để vượt map. Respawn do game vận hành bình thường. `route-results.json`.
- Hai tiến trình test cuối đều thoát **exit code 0** bằng `-batchmode`. `game-exit.txt`, `route-exit.txt`.
- Ảnh PNG là frame thật từ game camera + UI, render vào RenderTexture bằng URP SubmitRenderRequest, không phải mockup. Đã xem Home, gameplay, pulse/choáng, bệ động, checkpoint trên cao và màn kết thúc.

## Giới hạn kiểm tra

Desktop Windows đã khóa trong lúc kiểm tra, nên chưa chơi bằng bàn phím/chuột vật lý hoặc đóng cửa sổ thông thường. Mở player với cửa sổ ẩn (`Start-Process -WindowStyle Hidden`) trong phiên khóa tái hiện access violation ở UnityPlayer.dll lúc shutdown; không xuất hiện trong hai lượt batchmode cuối. Đây là kết quả quan sát, chưa khẳng định nguyên nhân ở game, engine hoặc desktop. Người dùng nên mở `Cuoi.exe` bình thường sau khi mở khóa máy để kiểm tra cửa sổ tương tác.

Log có các lỗi Curl/Unity TLS từ runtime Unity trong môi trường hiện tại; game không có tính năng mạng và các kiểm tra gameplay vẫn đạt. Không có managed exception trong hai log test cuối.

Thời gian bot không đại diện người chơi mới. Mục tiêu thiết kế là khoảng 90–150 giây khi đọc hướng dẫn và quan sát nhịp bệ; cần playtest người thật để chốt mức ~2 phút. Kỹ năng bám vách mới là cờ phần thưởng cho màn 2, chưa phải cơ chế có thể dùng trong màn 1.
