# Trận Cây Đa · Đồi Gốc Đa

Cuội nhặt mảnh trăng thứ tư, hồi đầy ba tim và gặp Cây Đa ngay bên cột hồi sinh cuối màn. Mục tiêu là thắp sáu mầm bằng lồng đèn. Mỗi mầm chỉ nhận ánh sáng khi cây vừa hoàn tất một chuỗi đòn. Khi mất cả ba tim và chơi lại từ cột, các mầm đã thắp vẫn sáng và cây lấy đà lại.

| Đòn | Dấu hiệu | Cách vượt |
|---|---|---|
| Xung phong | Cây nghiêng về sau, tiếng gỗ căng, một hàng vệt vàng trên mặt đất | Lướt xuyên đầu rễ hoặc nhảy qua khi cây tiến đến. |
| Quét rễ thấp | Cành hạ xuống, hàng dấu màu cam ngang đầu gối | Nhảy khi cành vung tới hoặc lùi khỏi tầm quét. |
| Quét tán cao | Cành nâng ngang tán, hàng dấu xanh ngọc ở trên đầu Cuội | Đứng hoặc chạy sát đất để cành lướt qua đầu. |
| Dậm gốc | Hai tiếng trống gỗ, một cụm dấu tụ trên nền ngay vị trí được chọn | Rời dấu đất trước khi cây tiếp đất rồi nhảy qua đầu sóng gần nhất. |

Cây dạy riêng từng đòn xung phong và dậm gốc trong hai lần mở mầm đầu. Rễ thứ hai phối thấp–cao và xung phong–dậm. Rễ cuối dùng thấp–cao–thấp, tiếp theo là hai lượt xung phong đổi chiều rồi dậm gốc. Mỗi đòn có thế lấy đà riêng. Mỗi lần hở thế kéo dài 5,25 giây. Mầm nở cao dần trên thân, ở ngay vị trí cây dừng chân.

## Nguồn thiết kế

Tham khảo cách Hand of the King trong *Dead Cells* báo trước hướng xông, nối đòn đánh và phát sóng mặt đất. Cây Đa có hình, chuyển động, âm thanh và điều kiện chiến thắng viết riêng cho Cuội. [Motion Twin giới thiệu Hand of the King](https://www.youtube.com/watch?v=d1fobWYpmd4) · [Các đòn của Hand of the King](https://deadcells.wiki.gg/wiki/Hand_of_the_King)

## Nguồn hình và âm thanh

Hai hình do OpenAI ImageGen tích hợp trong Codex tạo ngày 25/11/2026, xuất PNG có alpha. Cây tham chiếu sprite Cây Đa của chính project trong bản 2.0. Rễ được tạo trực tiếp từ mô tả. Unity nhập với bộ lọc Point. [Sprite cây](../Assets/Moonlit/Resources/BanyanGuardian.png) · [Sprite rễ](../Assets/Moonlit/Resources/BanyanRootArm.png)

**Prompt cây**

> Game-ready SINGLE full-body sprite for a Vietnamese moon festival pixel-art platformer. Replacement for the referenced banyan guardian. Preserve its identity as an ancient rooted Vietnamese banyan with hanging aerial roots, a broad asymmetric teal canopy, a stout braided trunk and a warm lanternlit grounding root fan. REDESIGN the entire face to be the simplest possible friendly wood spirit glyph: exactly two very small solid square golden pixels/rectangles for eyes above a short horizontal two-step groove smile carved in a flat dark area of bark. Eyes are blunt gold marks with no pupils or sockets. No human facial anatomy whatsoever: no nose, nostrils, eyebrows, eyelids, skin, beard or lips. Make the trunk read entirely as a tree even when zoomed in. Big and impressive but kind. Hand-placed 16-bit pixel art with deliberately chunky, unmistakably square pixel clusters like a historical 144x144 pixel sprite enlarged nearest-neighbor. Original stylized gameplay silhouette with two short, powerful upward-curving knotted side limbs and small warm cinnabar cloth charms tied to a branch, three blocky masses of leaves, cool indigo shadows, amber edges. Side-view platformer front three-quarter view facing slightly left, stout grounded foot; full crown and root tips visible in a single near square, centered occupying 94% of frame. Transparent backdrop with clean alpha, no ground tile, no shadow plane, no detached sprites or objects, no text, no game logo, no panel, no human/costume, no weapons, no realism. Keep face small, sweet and readable at game size.

**Prompt rễ**

> One isolated game sprite for a Vietnamese banyan tree spirit, transparent background. Side-on strict horizontal profile of ONE long tapered, powerful braided banyan root, thick cut base at far LEFT edge, tapering into an upward hooked golden root tip at far RIGHT. Full item fits. Its silhouette is like a thick irregular fallen branch (8:2 width to height), not a weapon: twisting dark umber wood, two root strands wrapping tightly, old golden facets of exposed bark on the upper rim, three tiny muted jade leaf sprigs and one cinnabar cloth wrapping near the base. Handmade arcade pixel art in sharply stepped large pixel clusters, about 100x25 virtual pixels scaled up. Gentle moonlight from upper left; graphic and readable from far away. Edges clean, front orthographic 2D platformer. Single item across a WIDE landscape canvas. Absolutely no surrounding sparks, no duplicate side views, no text or shadow or ground. The color and rendering belong to a friendly fantasy tree. True transparent alpha.

Bốn âm thanh nguyên gốc gồm tiếng gỗ siết trước cú xông, tiếng trống gỗ báo dậm, tiếng gió lúc xông và tiếng va đất trầm. Chúng được tổng hợp từ dao động và nhiễu, xuất WAV mono 24 kHz. Dùng `python Tools/ScoreMoonlit.py --guardian` để tái tạo. Nhạc nền trận đấu vẫn dùng bản phối piano *Trống quân*. [Mã tạo bốn tiếng động](../Tools/ScoreMoonlit.py) · [Nguồn nhạc](FESTIVAL_PIANO.md)
