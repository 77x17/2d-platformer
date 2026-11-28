# Polish khả năng quan sát — 19/11/2026

Chỉ cập nhật source và asset để Play trong Unity theo yêu cầu; các `.exe` hiện có vẫn là phiên bản trước.

## Vật phẩm và ánh sáng

- Thay 40 hạt xanh bằng **đồng trăng vàng** có viền tối, trăng khuyết màu ngà, cao 0.54 world unit (hình thoi cũ 0.13). Vật phẩm luôn hiện thân rõ, bob 0.075 unit và nghiêng tối đa 5 độ; chỉ điểm lóe ở vành xuất hiện theo nhịp.
- Khi nhặt: sprite bật lên rồi tan trong 0.38s, vòng vàng nhỏ, 6 hạt phụ và icon HUD nảy. HUD dùng cùng texture. Đom đóm chuyển sang xanh dịu, nhỏ và ít sáng hơn.
- Giữ 40 vị trí, trigger bán kính 0.42 và mỗi hạt chỉ cộng điểm một lần. Artwork bob; trigger đứng yên.
- `PlayerTuning.ambientVisibility = 0.30`: shader phủ đen alpha 0.70 ngoài đèn, giữ 30% màu môi trường trước lớp che. Tutorial vẫn tối hoàn toàn ngoài spotlight để tập trung đọc.

## Platform và bàn chân

`MossPlatforms.png` gồm bốn biến thể: đá phiến, đá có khoáng sáng/rễ, gạch cổ và đá tròn bám rễ. Cùng palette với atlas gốc; mặt đi phẳng, đổi thứ tự/lật ngang/sắc độ nhẹ để bớt lặp. Gỗ và các prop tiếp tục dùng art gốc.

Importer tìm hàng alpha liền mạch để đặt **pivot đúng mặt đi**, không đặt theo đỉnh cỏ/đỉnh ảnh. Tile nằm đúng Y của collider, rêu phía trên chỉ còn vài pixel. Bỏ độ nâng 0.35 unit của tile cũ.

`PlatformSurface` tách dải rêu mỏng từ cùng sprite bằng Sprite slicing: nền đá order 3 → nhân vật 10 → mép rêu 11. Chỉ phần thấp nhất của chân bị che; bụi cây lớn chuyển ra sau người. Độ lún trên rêu **0.065 unit**, trên gỗ **0.012**, trên không **0**. Collider và tuyến platform không đổi. Đây là tách lớp mặt đất/tiếp xúc, chưa cắt chân khỏi sprite hoặc thêm skeletal IK.

## Play test

Sau khi Unity refresh và compile, importer tự gắn hai asset vào `ArtLibrary`. Có thể chạy thủ công: **Moonlit → 4 - Import readable tokens and platform surfaces**. Mở `Assets/Moonlit/Scenes/01_Bamboo_Village.unity` rồi Play; không cần tạo lại scene hoặc build.

Runtime và Editor C# đã compile thành công bằng Roslyn đi kèm Unity 6000.3.16f1, dùng reference của project. Hai PNG mới có alpha trong suốt. Log: `QA/Readability/compile.log`. Chưa chạy gameplay/render mới hoặc build Windows; báo cáo 58/58 ngày 18/11 thuộc bản trước.

Khi Play: so độ rõ vật phẩm với đom đóm, nhìn đường ngoài vùng đèn, nhặt/restart, đứng/chạy trên rêu, đáp bệ gỗ lên/xuống và mở tutorial. Chỉnh ánh sáng/độ lún trong `Assets/Moonlit/Settings/PlayerTuning.asset`.

Asset dùng công cụ **image_gen tích hợp**, PNG lưu nguyên bản tại `Assets/Moonlit/Art/MoonToken.png` và `Assets/Moonlit/Art/MossPlatforms.png`. Prompt đầy đủ: [readability-asset-prompts.md](readability-asset-prompts.md).
