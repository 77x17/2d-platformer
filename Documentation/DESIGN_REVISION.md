# Điều chỉnh trải nghiệm màn 1 — 18/11/2026, bản Refined

## Hướng thiết kế

Giữ bản sắc của bản đầu: rừng xanh đêm, đá phủ rêu, rễ Đa và gỗ cũ, ánh đèn ấm. Gameplay chính là quan sát → chọn thời điểm → nhảy/né → nghỉ ở vùng an toàn. Không thêm thao tác đẩy thùng trong phiên bản này.

Atlas `Environment.png` gốc lại là nguồn art cho toàn bộ địa hình, cầu và cành cây. Bộ `ForestExtras.png` được giữ trong project để tham khảo, chỉ các sprite lá nhỏ còn dùng trong gió. Không trộn bè tre vàng, đá chạm khắc mới và dây thừng phóng to vào địa hình nữa.

## Tutorial có trọng tâm

Sáu bài ngắn, mỗi bài gắn với đối tượng đang gặp:

| Bài | Vùng an toàn để mở | Đối tượng được chiếu sáng | Nội dung |
|---|---|---|---|
| Điều khiển | Điểm xuất phát, sau khi đứng đất | Cuội và đèn | A/D; giữ/nhả Space |
| Chó | X≈29 trên bãi đá rộng | Chó và dấu ! | Nhìn báo trước, F làm choáng hoặc nhảy qua |
| Bệ nâng | X≈74 trước mép vực | Bệ gỗ treo | Đợi nhịp thấp, đáp giữa, đứng yên để bệ mang theo |
| Mèo | X≈102, phần đầu bãi nghỉ | Mèo và dấu ! | Vồ cao/xa, giữ khoảng cách và dùng đèn |
| Cầu sụp | X≈111, trước mép cầu | Nhịp gỗ mục đầu tiên | Có 0.7s trước khi ván rơi, nhảy tiếp |
| Miếu | X≈169 trên đất chắc | Miếu đèn | E mở cầu cho phần còn lại của lượt chơi |

Khi tutorial mở, state chuyển sang `Tutorial`, Time.timeScale=0. Nhân vật, quái, bệ, cooldown và đồng hồ gameplay dừng. Camera chuyển có nội suy tới đối tượng; shader chuyển vùng sáng từ lồng đèn sang đối tượng. HUD thường ẩn để người chơi tập trung vào vật được chỉ và thẻ hướng dẫn. Quái hiển thị dấu ! mẫu trong bài học.

Fade/camera/UI dùng unscaled time. Hướng dẫn không tự hết hạn: người chơi chọn **Đã hiểu**, Enter hoặc Space để tiếp tục. Khóa xác nhận 0.85s đầu tránh việc phím đang bấm làm đóng hướng dẫn ngay khi xuất hiện. Khi đóng, fade và camera trở về trong 0.45s, sau đó mới chạy gameplay; input nhảy được xóa để không nhảy ngoài ý muốn. Cuội có 0.65s bảo vệ khi trở lại để tránh bị quái chạm ngay lúc vừa đọc xong.

Chỉ kích hoạt bài khi nhân vật đang grounded ở vùng an toàn. Đã đọc thì không lặp lại sau khi chết; chơi lại từ đầu sẽ reset. Chuyển Home hoặc restart trong khi modal mở cũng trả timeScale về 1, không để game bị khóa.

Tham khảo: [Game Accessibility Guidelines — cho phép tự quyết tốc độ đọc](https://gameaccessibilityguidelines.com/allow-players-to-progress-through-text-prompts-at-their-own-pace/) và [hướng dẫn cơ chế trong bối cảnh gameplay](https://gameaccessibilityguidelines.com/include-interactive-tutorials/). Đây là cơ sở cho việc không dùng toast tự biến mất để dạy cơ chế quan trọng.

## Cảm giác đứng, idle và chạy

- **Idle:** dùng một frame đăng ký chân ổn định từ bộ cũ; nhịp thở chỉ đổi tỷ lệ Y ±0.3%, chân giữ nguyên pivot. Tránh phát liên tục bốn pose AI lệch silhouette/đầu/chân gây rung.
- **Foot planting / sprite grounding:** hình Cuội hạ 0.085 world unit so với collider để chân ăn nhẹ vào rêu. Không dìm collider xuống mặt đất. Thêm contact shadow nhỏ dưới chân khi grounded.
- **Mặt đi:** sprite đá/rêu được nâng để hàng rêu khớp với mặt collider. Bệ gỗ căn theo mặt ván đi được, không căn theo đỉnh cọc lan can.
- **Giảm trượt:** ground deceleration 110 unit/s²; animation chạy tiến frame theo quãng đường thực đi (0.58 unit/frame), không theo đồng hồ toàn cục. Khi dừng về pose idle ngay.
- **Ổn định vật lý:** không đặt lại vị trí Rigidbody đang nằm yên trên đất mỗi fixed step; để solver giữ contact. Chỉ chỉnh khi cần snap hoặc đang được bệ động mang theo. Giữ interpolation để làm mượt hiển thị. [Unity Rigidbody2D interpolation](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/rigidbody2d/interpolation).
- Coyote time 0.10s và jump buffer 0.14s vẫn giữ; giải thích thuật ngữ ở `POLISH_REPORT.md`.

Đây là xử lý hình ảnh/pivot và điều khiển của nhân vật sprite, chưa phải skeletal foot IK.

## Thiết kế lại bệ và nhịp màn

Tuyến đầu trở lại chuỗi bậc rêu với cao độ tăng vừa phải, hạ xuống bãi checkpoint. Gỡ hoàn toàn spawn thùng, bàn đạp, cầu phụ, chỉ dẫn và logic push trong controller. Code/atlas thùng được giữ ở project như vật liệu dự phòng, không tham gia màn chơi.

Giảm bệ chuyển động từ 4 xuống **2**, cả hai là bệ gỗ nâng dọc dưới rễ Đa. Bệ đầu biên độ 0.75 unit, chu kỳ 4.8s; bệ sau biên độ 0.55 unit. Dây treo mảnh có đầu trên cố định tại cành, đầu dưới theo bệ. Các bệ ngang lơ lửng được đổi thành bệ rêu tĩnh có rễ phía dưới. Bệ sụp vẫn dùng gỗ cũ cùng atlas, biểu đạt bằng rung và ngả màu trước khi rơi.

Mỗi cơ chế được giới thiệu trên vùng có chỗ đứng, sau đó mới kết hợp với cơ chế khác. Giữ 5 checkpoint, 5 nhịp sụp, 6 encounter, 40 đốm trăng tùy chọn và mảnh trăng kết màn. Vùng đèn vẫn giới hạn tầm nhìn; tutorial giúp nhận diện nguy hiểm trước lần đầu vượt qua.

## Biên trái

Đá và rễ Đa đánh dấu đầu đường. Collider tường chặn bên trái, cùng giới hạn an toàn X=1.25 cho player. Đã bỏ điều kiện `x < -2 => Hurt(true)`; đi trái không mất máu/hồi sinh. Rơi xuống đáy vực vẫn hồi sinh như trước.

## Điểm chỉnh cho programmer

| Nội dung | File |
|---|---|
| Trigger, nội dung, trạng thái tutorial | `MoonlitTutorial.cs` |
| Thẻ hướng dẫn và nút xác nhận | `MoonlitHud.cs` |
| Camera focus/return | `MoonlitCamera.cs` |
| Spotlight đổi từ đèn sang đối tượng | `LanternDarkness.cs`, `LanternDarkness.shader` |
| Idle, quãng đường animation, chân, giảm trượt | `CuoiController.cs`, `PlayerTuning.cs` |
| Layout, art gốc, giới hạn trái | `MoonlitLevel.cs` |
| Dây neo cố định của bệ nâng | `LiftSuspension.cs` |

Regression riêng: `-moonlitRevisionQA`; nền tảng: `-moonlitQA`, `-moonlitPolishQA`; route bật tutorial thật: `-moonlitRouteQA`. Các báo cáo thực tế lưu trong `QA` sau khi kiểm tra.
