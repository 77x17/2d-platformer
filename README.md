# Cuội — Đường Về Trăng

Một màn platformer 2D cuộn ngang trong Unity **6000.3.16f1**. Chạy, nhảy, tránh thú rừng và tương tác môi trường; không có vũ khí, chặt chém hay hệ thống gây sát thương cho quái.

## Chạy game

- **Code mới 19/11:** Play trong Unity để thử đồng trăng vàng, vùng tối giữ sáng 30%, texture platform mới và lớp rêu chỉ che chân. Asset tự import sau compile; có thể chạy menu **Moonlit → 4 - Import readable tokens and platform surfaces**. Chi tiết: `Documentation/READABILITY_POLISH.md`. Lần cập nhật này không build `.exe`.
- Bản `.exe` gần nhất: `Builds/Windows-Refined/Cuoi.exe` (18/11, chưa có thay đổi 19/11). Giữ nguyên toàn bộ thư mục Windows-Refined cạnh file exe.
- Unity: mở `Assets/Moonlit/Scenes/Level01_BanyanForest.unity`, nhấn Play.
- Nếu cần khởi tạo lại: menu **Moonlit → 1 - Create or refresh level 1**. Lệnh này tái tạo scene bootstrap và cắt lại atlas AI; không chạy sau khi đã tùy chỉnh scene nếu chưa lưu bản riêng.
- Build: **Moonlit → 2 - Build Windows executable**. Build bình thường giữ art library / tuning hiện có.
- Menu build vẫn xuất vào `Builds/Windows`; đóng player ở thư mục đó trước khi build. `Windows-Refined` là snapshot bàn giao; `Windows-Polish` là bản cũ còn puzzle thùng.

| Phím | Thao tác |
|---|---|
| A / D hoặc ← / → | Chạy trái / phải |
| Space, W hoặc ↑ | Nhảy; giữ để nhảy cao, nhả để nhảy thấp |
| F hoặc J | Lồng đèn bừng sáng, làm choáng quái trong 2,6 giây; hồi 3 giây |
| E | Thắp đèn ở miếu cạnh cây cầu bị ngắt |
| Esc | Tạm dừng / tiếp tục |
| R | Hồi sinh tại đèn gần nhất |
| M | Bật / tắt âm thanh |
| Enter | Bắt đầu / tiếp tục / chơi lại tùy màn hình |
| Enter / Space hoặc nút Đã hiểu | Đóng tutorial sau khi đọc, trả camera về Cuội |

## Nội dung đã triển khai

- Cuội có idle / run / jump / lantern, mỗi hàng atlas 4 frame.
- Hai loại thú rừng: chó tuần tra rồi lao tới; mèo báo trước rồi vồ. Cả hai có idle / walk / pounce / stunned, không bị giết.
- 1 tuyến đường 370 world units, nhiều cao độ; 5 checkpoint, 2 bệ gỗ nâng dọc có dây treo, 5 bệ sụp tự phục hồi, cầu tương tác, 40 đốm trăng tùy chọn và 1 mảnh trăng kết màn.
- 6 tutorial spotlight: gameplay dừng, camera focus đối tượng, vùng khác tối, người chơi tự xác nhận khi đọc xong.
- Animation môi trường: dây leo / tre / lá / đèn lắc, đom đóm trôi và nhấp nháy, mảnh trăng bob/rotate.
- Polish: coyote time, jump buffer, gia tốc/giảm tốc, variable jump, squash khi nhảy/tiếp đất, bụi chân, vòng sáng, hạt sáng, camera follow/lookahead/shake, fade hồi sinh và âm thanh tổng hợp nhẹ.
- Idle dùng pose ổn định và nhịp thở nhẹ; chân lún 0.065 unit trên rêu, 0.012 trên gỗ; rêu tiền cảnh chỉ che bàn chân; contact shadow; animation chạy theo quãng đường; nhả phím dừng nhanh. Biên trái chặn vật lý, không gây chết.
- Home, logo chữ, Play, HUD, pause, kết màn; text tiếng Việt.

## Điểm sửa dành cho programmer

| Muốn sửa | File / asset |
|---|---|
| Tốc độ, jump, gravity, thời gian choáng | `Assets/Moonlit/Settings/PlayerTuning.asset` |
| Thay ảnh / frame | `Assets/Moonlit/Settings/ArtLibrary.asset` |
| Bố cục màn, tọa độ và encounter | `Assets/Moonlit/Scripts/MoonlitLevel.cs` |
| Controller / nhảy / lantern | `Assets/Moonlit/Scripts/CuoiController.cs` |
| Hành vi chó / mèo | `Assets/Moonlit/Scripts/ForestCreature.cs` |
| Bệ, cầu, checkpoint, pickup | `Assets/Moonlit/Scripts/ForestMechanisms.cs` |
| Tutorial và nội dung hướng dẫn | `Assets/Moonlit/Scripts/MoonlitTutorial.cs` |
| Dây neo bệ nâng | `Assets/Moonlit/Scripts/LiftSuspension.cs` |
| Bóng tối quanh đèn | `Assets/Moonlit/Resources/LanternDarkness.shader`, `Scripts/LanternDarkness.cs` |
| Gió và nền cuộn | `Assets/Moonlit/Scripts/ForestWind.cs`, `MoonlitCamera.cs` |
| UI / typography / nội dung | `Assets/Moonlit/Scripts/MoonlitHud.cs` |
| Sprite slicing / build | `Assets/Moonlit/Editor/LevelOneBuilder.cs` |

Scene hiện dùng bootstrap để tạo hierarchy lúc Play. Các thông số art và controller là ScriptableObject; layout nằm trong code có chia 4 đoạn rõ ràng. Có thể chuyển các nhóm runtime thành prefab/tilemap ở bước tiếp theo. Không thay scene mẫu gốc `Assets/Scenes/SampleScene.unity`.

## Phạm vi và lưu ý

Chỉ màn 1 được làm. Chưa có intro hoặc màn 2–4. Nhận mảnh trăng lưu cờ `Moonlit.WallClimbUnlocked` cho màn 2; **chưa triển khai cơ chế bám/leo vách** trong màn 1. Thời lượng thiết kế khoảng 90–150 giây cho người chơi lần đầu, cần lấy mẫu playtest người thật để chốt mốc 2 phút.

Ánh sáng dùng shader radial mask giữ 30% màu môi trường ngoài đèn (tutorial vẫn tối hoàn toàn ngoài spotlight), kết hợp glow/ripple; chưa tính bóng đổ sau vật cản. Background dùng 3 tile từ ảnh rừng AI, cuộn ngang liên tục và parallax theo camera; các lớp cây/dây leo/lá nhận sự kiện gió riêng. Âm thanh là clip tổng hợp bằng code, không dùng nhạc tải từ bên thứ ba.

## Kiểm tra

`Cuoi.exe -batchmode -moonlitQA -force-d3d11 -screen-width 1600 -screen-height 900 -logFile "<đường dẫn log>"` chạy các kiểm tra tích hợp trong build bằng Input System, cùng controller và Physics2D. Fixture dùng teleport để cô lập từng cơ chế. Thay `-moonlitQA` bằng `-moonlitRouteQA` để chạy một lượt từ đầu đến cuối bằng điều khiển bình thường, không teleport hoặc bất tử. Báo cáo JSON và ảnh camera nằm trong `Builds/QA/` (hai cấp trên `Cuoi_Data`). Chế độ bình thường không bật QA.

Các bộ kiểm tra hiện tại: `-moonlitQA` (18 kiểm tra), `-moonlitPolishQA` (18 kiểm tra sau khi gỡ thùng), `-moonlitRevisionQA` (22 kiểm tra tutorial/idle/biên trái), `-moonlitRouteQA` (đi liên tục qua đủ 6 tutorial). Xem kết quả bản mới tại `QA/REFINED_VERIFICATION.md`. Bot biết sẵn đường nên thời gian route không đại diện thời lượng người chơi mới. Các file `POLISH_VERIFICATION.md` và `VERIFICATION.md` là lịch sử phiên bản trước.

Package skeletal animation `com.unity.2d.animation` không được dùng bởi animator đổi frame của game này và đã bỏ khỏi manifest; package cũ cấp một GPU fallback buffer dù không có SpriteSkin. Nếu bạn muốn chuyển sang animation xương, có thể cài lại package tương thích từ Package Manager.

Xem `Documentation/LEVEL_DESIGN.md` cho ý tưởng 4 màn và `Documentation/ASSETS.md` cho hợp đồng thay asset. Các API vật lý được đối chiếu với [Unity Rigidbody2D](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/rigidbody2d).
## Bản Refined 18/11

Quay về atlas môi trường gốc để thống nhất đá/rêu/gỗ. Gỡ puzzle thùng, giảm bệ động còn 2 bệ treo có điểm neo rõ; tuyến đầu trở lại bậc rêu. Hướng dẫn quan trọng có spotlight và tạm dừng; sửa rung idle, cảm giác chân và biên trái.

Thiết kế và kỹ thuật hiện tại: [Documentation/DESIGN_REVISION.md](Documentation/DESIGN_REVISION.md). Thuật ngữ coyote time/jump buffering dùng trong báo cáo: [Documentation/POLISH_REPORT.md](Documentation/POLISH_REPORT.md).

