# Cuội — Đường Về Trăng

Một màn platformer 2D cuộn ngang trong Unity **6000.3.16f1**. Chạy, nhảy, tránh thú rừng và tương tác môi trường; không có vũ khí, chặt chém hay hệ thống gây sát thương cho quái.

## Chạy game

- Bản Windows: `Builds/Windows/Cuoi.exe`. Giữ nguyên toàn bộ thư mục Windows cạnh file exe.
- Unity: mở `Assets/Moonlit/Scenes/Level01_BanyanForest.unity`, nhấn Play.
- Nếu cần khởi tạo lại: menu **Moonlit → 1 - Create or refresh level 1**. Lệnh này tái tạo scene bootstrap và cắt lại atlas AI; không chạy sau khi đã tùy chỉnh scene nếu chưa lưu bản riêng.
- Build: **Moonlit → 2 - Build Windows executable**. Build bình thường giữ art library / tuning hiện có.

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

## Nội dung đã triển khai

- Cuội có idle / run / jump / lantern, mỗi hàng atlas 4 frame.
- Hai loại thú rừng: chó tuần tra rồi lao tới; mèo báo trước rồi vồ. Cả hai có idle / walk / pounce / stunned, không bị giết.
- 1 tuyến đường 370 world units, nhiều cao độ; 5 checkpoint, 4 bệ chuyển động, 5 bệ sụp có tự phục hồi, cầu tương tác, 40 đốm trăng tùy chọn và 1 mảnh trăng kết màn.
- Animation môi trường: dây leo / tre / lá / đèn lắc, đom đóm trôi và nhấp nháy, mảnh trăng bob/rotate.
- Polish: coyote time, jump buffer, gia tốc/giảm tốc, variable jump, squash khi nhảy/tiếp đất, bụi chân, vòng sáng, hạt sáng, camera follow/lookahead/shake, fade hồi sinh và âm thanh tổng hợp nhẹ.
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
| UI / typography / nội dung | `Assets/Moonlit/Scripts/MoonlitHud.cs` |
| Sprite slicing / build | `Assets/Moonlit/Editor/LevelOneBuilder.cs` |

Scene hiện dùng bootstrap để tạo hierarchy lúc Play. Các thông số art và controller là ScriptableObject; layout nằm trong code có chia 4 đoạn rõ ràng. Có thể chuyển các nhóm runtime thành prefab/tilemap ở bước tiếp theo. Không thay scene mẫu gốc `Assets/Scenes/SampleScene.unity`.

## Phạm vi và lưu ý

Chỉ màn 1 được làm. Chưa có intro hoặc màn 2–4. Nhận mảnh trăng lưu cờ `Moonlit.WallClimbUnlocked` cho màn 2; **chưa triển khai cơ chế bám/leo vách** trong màn 1. Thời lượng thiết kế khoảng 90–150 giây cho người chơi lần đầu, cần lấy mẫu playtest người thật để chốt mốc 2 phút.

Ánh sáng lồng đèn dùng sprite glow và ripple, không phải hệ chiếu sáng động có đổ bóng. Background là một plate AI có chuyển động parallax nhẹ; các lớp cây/dây leo được đặt riêng. Âm thanh là clip tổng hợp bằng code, không dùng nhạc tải từ bên thứ ba.

## Kiểm tra

`Cuoi.exe -batchmode -moonlitQA -force-d3d11 -screen-width 1600 -screen-height 900 -logFile "<đường dẫn log>"` chạy các kiểm tra tích hợp trong build bằng Input System, cùng controller và Physics2D. Fixture dùng teleport để cô lập từng cơ chế. Thay `-moonlitQA` bằng `-moonlitRouteQA` để chạy một lượt từ đầu đến cuối bằng điều khiển bình thường, không teleport hoặc bất tử. Báo cáo JSON và ảnh camera nằm trong `Builds/QA/` (hai cấp trên `Cuoi_Data`). Chế độ bình thường không bật QA.

Test trên máy hiện tại: khởi chạy player cửa sổ ẩn khi desktop đang khóa gây lỗi native `UnityPlayer.dll / 0xc0000005` lúc đóng; không có lỗi C# trong gameplay. Chạy kiểm thử bằng `-batchmode` kết thúc mã 0. Cửa sổ tương tác thông thường chưa được xác nhận trực tiếp vì desktop đang khóa. Xem `QA/VERIFICATION.md`.

Package skeletal animation `com.unity.2d.animation` không được dùng bởi animator đổi frame của game này và đã bỏ khỏi manifest; package cũ cấp một GPU fallback buffer dù không có SpriteSkin. Nếu bạn muốn chuyển sang animation xương, có thể cài lại package tương thích từ Package Manager.

Xem `Documentation/LEVEL_DESIGN.md` cho ý tưởng 4 màn và `Documentation/ASSETS.md` cho hợp đồng thay asset. Các API vật lý được đối chiếu với [Unity Rigidbody2D](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/rigidbody2d).
