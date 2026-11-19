# Màn 1 — ánh sáng, chuyển động và tương tác (18/11/2026)

> Ghi chú phiên bản: phần thùng và các bệ từ ForestExtras bên dưới là lịch sử bản Polish. Bản Refined đã gỡ thùng, quay về art môi trường gốc, thêm tutorial spotlight và sửa idle/tiếp đất. Xem [DESIGN_REVISION.md](DESIGN_REVISION.md) cho trạng thái hiện tại. Coyote time và jump buffer trong mục thuật ngữ vẫn áp dụng.

## Thuật ngữ dùng trong báo cáo

| Thuật ngữ | Ý nghĩa | Áp dụng trong game |
|---|---|---|
| **Coyote time / ledge grace period** | Một khoảng dung sai ngắn sau khi nhân vật đi khỏi mép bệ, trong đó lệnh nhảy vẫn hợp lệ. | `PlayerTuning.coyoteTime = 0.10` giây; khoảng 6 bước vật lý ở 60 Hz. |
| **Jump buffering / input buffer** | Ghi nhớ lệnh nhảy được bấm ngay trước khi chạm đất, thực hiện khi điều kiện nhảy hợp lệ. | Giữ cửa sổ 0.14 giây. Khác với coyote time: buffer lưu input trước khi đáp; coyote giữ quyền nhảy sau khi rời mép. |
| **Variable jump height** | Giữ/nhả nút làm thay đổi độ cao cú nhảy. | Giữ Space để nhảy cao; nhả sớm giảm vận tốc đi lên. |
| **Attack telegraph / anticipation** | Tín hiệu báo trước đòn đánh, tạo thời gian để người chơi phản ứng. | Dấu `!` trắng trên hình thoi vàng có viền tối, nảy/phồng khi xuất hiện: chó 0.75 giây, mèo 0.90 giây trước khi lao. |
| **Parallax scrolling** | Các lớp cảnh dịch chuyển với tốc độ khác nhau để tạo chiều sâu. | Nền rừng cuộn ngang bằng 3 tile, tốc độ tương đối theo camera 0.22; thêm trôi ngang theo gió dù Cuội đứng yên. |
| **Ambient animation / environmental event** | Chuyển động phụ và sự kiện môi trường giúp cảnh sống động. | Lá bay ngang, cành/rễ/tre lay động, các đợt gió có tăng/giảm cường độ. |
| **Radial light mask / soft falloff** | Mặt nạ khoảng cách tròn với biên sáng suy giảm mềm. | Shader HLSL/URP phủ đen thế giới bên ngoài đèn, HUD vẫn hiển thị. |
| **Pressure plate puzzle** | Dùng vật nặng giữ bàn đạp để vận hành cơ cấu. | Đẩy thùng gỗ đến bàn đạp ở x=54.5 để hạ cầu gỗ. |

Nguồn giải thích coyote time: [KidsCanCode — Coyote Time, kèm project mẫu của tác giả](https://kidscancode.org/godot_recipes/4.x/2d/coyote_time/index.html). Nguồn mô tả cho phép nhảy trong vài frame sau khi đi khỏi mép. Các giá trị 0.10/0.14 giây ở đây là lựa chọn thiết kế của project, **không phải thông số đã xác minh của Dead Cells**.

Đoạn có thể dùng trong báo cáo:

> Trò chơi sử dụng coyote time 0,10 giây nhằm cho phép nhân vật nhảy trong một khoảng thời gian ngắn sau khi rời mép bệ. Cơ chế này bù cho sai lệch nhỏ giữa cảm nhận của người chơi và thời điểm va chạm vật lý kết thúc. Jump buffering 0,14 giây bổ sung khả năng ghi nhận thao tác nhảy ngay trước khi tiếp đất. Hai cơ chế cải thiện độ phản hồi của điều khiển mà không bổ sung nhảy đôi.

## Sửa nhận diện đứng trên bệ nâng

Bản cũ chỉ nhận ground khi vận tốc Y thế giới <= 0.4. Bệ kinematic đi lên có thể truyền vận tốc dương cho Rigidbody2D của Cuội, khiến nhân vật bị đánh dấu airborne dù chân đang đứng trên bệ; animation lấy trực tiếp trạng thái này nên giữ pose nhảy.

`CuoiController.FixedUpdate` mới:

1. Probe chân từ `Rigidbody2D.position`, tránh sai số từ Transform đang nội suy hình ảnh.
2. Kiểm tra normal hướng lên, khoảng cách chân đến mặt bệ, và **vận tốc Y tương đối với bệ**.
3. Khi grounded, mang nhân vật theo delta bệ đúng một lần và đặt chân theo mặt bệ của bước vật lý kế tiếp. Không parent Rigidbody động vào Transform bệ.
4. Khi nhảy, xóa support và tiêu thụ coyote time; khóa tái nhận ground trong 0.12 giây đầu cất cánh. Thời gian này là debounce tiếp xúc, khác với coyote time.
5. Animation idle/run/jump theo grounded sau bước vật lý; không chỉ dựa vào vận tốc Y thế giới.

Coyote time chỉ được nạp lại khi nhận mặt đất hợp lệ. Cú nhảy đã thực hiện sẽ tiêu thụ cửa sổ này; bấm lại trên không không tạo nhảy đôi. Thời gian được lượng tử hóa theo fixed timestep, nên biên chính xác có sai số tối đa khoảng một bước vật lý.

## Shader bóng tối

File: `Assets/Moonlit/Resources/LanternDarkness.shader`; điều khiển bởi `LanternDarkness.cs`.

Một quad phủ viewport, sorting order 900, vẽ sau toàn bộ world sprite/effect và trước UI. Fragment shader tính khoảng cách XY thế giới đến lồng đèn, rồi dùng `smoothstep(radius - softness, radius, distance)` làm alpha đen. Ngoài bán kính, alpha = 1 nên ngay cả đom đóm, miếu đèn, vật phẩm xa cũng bị bóng tối che. Home giữ nền trang trí; khi chơi bóng tối được bật.

- Bán kính thường: 6.4 world units; vùng biên mềm: 2.3.
- Bừng sáng F mở bán kính lên 9 rồi thu lại theo animation 0.45 giây.
- Tâm đèn theo hướng nhìn và động tác giơ đèn; dao động nhỏ tạo flicker.
- Tính khoảng cách bằng tọa độ thế giới giữ ánh sáng hình tròn khi đổi tỉ lệ cửa sổ.
- Đây là mask khám phá trong không gian 2D, chưa tính bóng đổ/che khuất bởi tường.
- Shader nằm trong Resources để được đưa vào Windows build; không phụ thuộc `Shader.Find` sau stripping.

Tham khảo cấu trúc shader: [Unity — basic unlit shader in URP](https://docs.unity.com/en-us/engine/6000.6/manual/materials-and-shaders/shaders/writing-custom-shaders-urp/writing-shaders-urp/basic-unlit-structure).

## Nền và sự kiện gió

`MoonlitCamera` dùng 3 tile liên tục, đổi tile theo vị trí camera; tile xen kẽ được mirror để nối hai mép tương ứng của ảnh nền gốc. `ForestWind` thêm drift liên tục và 34 lá tái sử dụng, trong đó chỉ 7 lá hiện khi trời lặng. Các lá còn lại xuất hiện khi gió tăng.

Gió đầu xuất hiện khi đi qua x=44; gió vùng rễ cao khi qua x=226; sau đó có đợt cách nhau ít nhất 26 giây gameplay. Mỗi đợt kéo dài 7 giây với envelope sin tăng rồi giảm, không bật/tắt đột ngột. Cành, lá, tre, dây leo nhận cùng cường độ với pha riêng. Pause dừng thời gian; restart reset event. Gió tác động hình ảnh, không tự ý đổi quỹ đạo nhảy của người chơi.

## Đa dạng bệ và thùng đẩy

Atlas bổ sung `ForestExtras.png`: bệ thân cây, bè tre buộc dây, đá chạm khắc, rễ đan, gốc cây, ván gỗ; thùng, bàn đạp, cành và lá. Bè tre dùng cho bệ di chuyển; ván gỗ cho bệ sụp; đá cổ/rễ cho vùng sau. Collider độc lập với sprite để thay art không đổi cảm giác nhảy.

Khu thùng ở x=44–65, trước checkpoint đầu:

1. Cuội đến cạnh thùng x=49 và giữ A/D để đẩy; không cần thêm nút.
2. Tốc độ đẩy 2.5 unit/s, thấp hơn chạy. Thùng có Rigidbody2D, trọng lực, va chạm và khóa xoay.
3. Thùng vào bàn đạp x=54.5 sẽ được giữ tại chỗ, cầu rộng 7.2 unit hạ trong 0.85 giây. Collider cầu chỉ mở khi hạ xong.
4. Người chơi nhảy qua thùng rồi sang cầu. Đáp trên thùng là hợp lệ.
5. Hồi sinh/restart trả thùng về vị trí đầu, khóa lại bàn đạp và nâng cầu. Thùng rơi khỏi màn cũng tự đặt lại.

Thông số chính nằm trong `PlayerTuning.asset`, `ForestWind.cs`, `ForestCreature.cs`, `CratePuzzle.cs` và layout `MoonlitLevel.cs`. Các sprite bổ sung nằm trong `ArtLibrary.asset.forestExtras`; menu import bổ sung giữ nguyên reference art cũ của người dùng.

## Kiểm tra hồi quy

`-moonlitPolishQA` kiểm tra coyote trong/ngoài cửa sổ, không nhảy đôi, nhảy và đáp trên bệ đang lên/xuống, đứng trên bệ qua 2 chu kỳ, nền trôi khi đứng yên, gió tăng/hạ, shader/pulse, dấu báo quái và hủy khi choáng, đẩy thùng mở cầu, nhảy qua thùng, reset khi chết/restart. Kết quả thực tế xem `QA/polish-results.json`; đi hết màn xem `QA/route-results.json`.
