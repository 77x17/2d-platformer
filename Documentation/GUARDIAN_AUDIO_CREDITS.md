# Cây Đa và âm thanh của Cuội Tìm Trăng

## Cây Đa thần

Cây Đa 2.1 có hai mắt vuông trên thân và một nét cười ngắn. Cánh rễ buộc vải đỏ theo thân khi xông, quét và dậm đất. [Nguồn hai sprite, prompt và âm thanh trận đấu](BANYAN_DUEL.md)

## Đèn ông sao

Sprite [StarLantern.png](../Assets/Moonlit/Resources/StarLantern.png) được tạo ngày 24/11/2026 bằng OpenAI ImageGen tích hợp trong Codex, theo mô tả đèn ông sao Trung Thu trong bản thiết kế. Không dùng ảnh tham chiếu. [CuoiHandLantern.cs](../Assets/Moonlit/Scripts/CuoiHandLantern.cs) đặt đèn vào đúng bàn tay của cả 16 khung chuyển động. Ngọn đèn và vùng sáng đi cùng nhau khi Cuội quay người hay nâng tay.

> Single game object sprite on fully transparent background, straight-on: one small traditional Vietnamese Mid-Autumn đèn ông sao lantern. Exactly five wide red vermilion translucent paper star points, thin golden bamboo outline, amber internal spokes and central glowing light, tiny hanging red tassel at the bottom. Five-point silhouette fills most of the square frame, upward top point, compact, warm and readable when scaled down to 28 pixels. Crisp 16-bit pixel art with a few deliberate pixel clusters, dark brown silhouette edges, no round outer hoop, no hanging cords above the top point, no hand, no pole, no text, no background or outer glow.

Ánh trăng mát trên vành nón và ánh đèn ấm ở phía tay được tính theo alpha của từng khung nhân vật trong [CuoiRim.shader](../Assets/Moonlit/Resources/CuoiRim.shader). [CuoiFootShadow.cs](../Assets/Moonlit/Scripts/CuoiFootShadow.cs) chiếu bóng chân và bóng đổ xuống mặt va chạm thật, thu bóng theo độ cao của cú nhảy và cắt bóng tại mép bệ. Hai hiệu ứng được viết riêng cho project.

Cuội có tám tư thế chạy, tám tư thế leo vách và bốn tư thế vượt mép bệ. Nguồn hình, ảnh tham chiếu và phần căn bàn chân, đầu ngón tay, đèn ở tay hoặc thắt lưng được ghi trong [tài liệu chuyển động](CUOI_ANIMATION_CREDITS.md).

Trên đường rước ở Sông Đèn và Đồi Gốc Đa, [LanternProcession.cs](../Assets/Moonlit/Scripts/LanternProcession.cs) dựng cột tre, thanh chống, nút buộc, chân đá và móc treo bằng hình học của Unity kết hợp sprite đá từ atlas môi trường có sẵn. Ba đèn dùng lại StarLantern.png và phát lần lượt ba câu chuông riêng.

## Nhạc và tiếng động

Sáu bản nhạc nền và hai câu kết phối lại làn điệu Trống quân (Bắc Bộ) và Đi cấy (Thanh Hóa) cho piano. Các nốt đàn thu âm của Alexander Holm được dùng theo giấy phép CC BY 3.0. [Bản ký âm, nguồn và bản phối từng cảnh](FESTIVAL_PIANO.md) · [Mã dựng piano](../Tools/FestivalPiano.py)

Ba mươi ba tiếng động được tổng hợp từ dao động, bao âm và nhiễu. [Mã tạo tiếng động](../Tools/ScoreMoonlit.py) · [Các tệp âm thanh](../Assets/Moonlit/Resources/MoonAudio/)

| WAV mono 24 kHz | Ngữ cảnh |
|---|---|
| sfx_jump, sfx_airJump, sfx_land, sfx_step, sfx_woodStep | Nhảy, nhảy đôi, tiếp đất, bước trên đất và trên gỗ |
| sfx_dash, sfx_climb, sfx_crate | Lướt, bám vách, đẩy thùng |
| sfx_pulse, sfx_ready, sfx_collect, sfx_checkpoint | Bùng sáng, đèn hồi, nhặt hạt trăng, thắp cột đèn |
| sfx_hurt, sfx_dog, sfx_cat, sfx_stun | Mất tim, chó gầm, mèo rít, quái bị choáng |
| sfx_rootCoil, sfx_rootDrum, sfx_rootRush, sfx_rootImpact | Rễ siết, trống báo dậm, gió xông ngang và tiếng dậm gốc. [Nguồn trận Cây Đa](BANYAN_DUEL.md) |
| sfx_carpBubbles, sfx_carpLeap, sfx_carpSplash, sfx_crowCall, sfx_crowFlight | Bọt nước, cá nhảy và lặn xuống, quạ kêu rồi vỗ cánh. [Nguồn sprite và tiếng động](WETLAND_CREATURE_CREDITS.md) |
| sfx_warn, sfx_sweep, sfx_bloom | Rễ báo hiệu, rễ quét, mầm nở |
| sfx_signal1, sfx_signal2, sfx_signal3 | Ba câu chuông nối nhau khi thắp các cột đèn trên đường rước |
| sfx_uiMove, sfx_uiClick | Chọn và kích hoạt nút |

Unity đọc các bản nhạc Ogg Vorbis và hiệu ứng WAV trực tiếp từ thư mục tài nguyên của game.
