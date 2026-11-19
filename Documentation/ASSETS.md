# Asset handoff

## Bản Refined hiện tại

Toàn bộ địa hình, bệ gỗ, rễ/cành và đạo cụ chính dùng lại `Environment.png` gốc. Atlas ForestExtras còn lưu để tham khảo; hiện chỉ ba sprite lá nhỏ được dùng cho gió. Không spawn thùng, bàn đạp, bè tre hoặc dây thừng của bản Polish. Dây treo bệ nâng được vẽ bằng LineRenderer mảnh, hai đầu nối theo cấu trúc cành–bệ. Không phát sinh ảnh AI mới trong lần sửa Refined.

## Bổ sung 18/11/2026

`ForestExtras.png` là atlas 4×4 mới, tạo bằng **image_gen tích hợp**, sau đó dùng image_gen chỉnh nền về alpha trong suốt và giữ nguyên 16 ô. Prompt gốc và bước chỉnh nền lưu ở `polish-asset-prompt.txt`. Thứ tự: log, bamboo, carved stone, roots, crate, plate, leaf, branch, rope, stump, steps, planks, golden leaf, teal leaf, fern, vine. Ô bậc thang được giữ làm prop dự phòng; các bệ gameplay dùng mặt đi ngang rõ ràng. `ArtLibrary.forestExtras` chứa 16 sprite và có thể thay độc lập với bộ cũ.

Importer dùng kích thước thực tế 1254×1254, alpha-trim và pivot top-center. Không resample/ghi đè PNG khi slice.

Tất cả 6 PNG được tạo bằng **imagegen tích hợp** trong phiên làm việc này và đã chép vào `Assets/Moonlit/Art/`. Không phụ thuộc đường dẫn generated_images ngoài project. Không lấy sprite từ itch.io.

| PNG | Layout | Cách dùng |
|---|---|---|
| Cuoi.png | 4 cột × 4 hàng | idle, run, jump, lantern; 4 frame/hàng |
| Dog.png | 4 × 4 | idle, walk, pounce, stunned |
| Cat.png | 4 × 4 | idle, walk, pounce, stunned |
| Environment.png | 4 × 4 | index 0–3 đá/rêu; 4 cầu; 5 cột; 6 đá; 7 dương xỉ; 8 dây leo; 9 tre; 10 nấm; 11 đèn; 12 miếu; 13 gai; 14 cành; 15 rễ |
| Forest.png | 1 ảnh | background đêm |
| MoonShard.png | 1 ảnh alpha | collectible và biểu tượng trên UI |

Atlas AI đầu ra có kích thước thực tế khác kích thước yêu cầu trong prompt. Importer đọc width/height thật và chia 4×4; không hardcode 256 pixel. `FilterMode.Point`, không mipmap, không compression. Sprite slice chỉ trim vùng trong cell và căn chân, không ghi đè ảnh PNG.

Nhân vật pivot ở chân, giữa theo chiều ngang; terrain/decor pivot top-center. PPU actor được tính từ kích thước cell để giữ tỷ lệ thế giới. Moon/background dùng center. Collider được giữ tách khỏi sprite, nên đổi ảnh không thay vật lý.

Thay asset: import PNG của bạn; slice animation; kéo 16 Sprite mới vào `ArtLibrary.asset` theo thứ tự row-major bên trên. Thay `forest`/`moon` hoặc `environment` tương ứng. Build bình thường không ghi lại các reference này. Menu **Create or refresh level 1** cố ý tái sinh atlas references từ các PNG gốc, nên chỉ dùng khi muốn dựng lại.

Các sheet là asset tạm để programmer chạy gameplay và kiểm tra animation. Chúng được đổi frame 5–11 FPS; đồng thời thêm squash, bụi chân, glow và camera để tạo cảm giác chuyển động. Khi làm bộ pixel art final nên thống nhất grid pixel, silhouette và vị trí lantern giữa các frame.

Prompt nguyên bản nằm trong `asset-prompts.json`. Audio ambient/feedback tổng hợp runtime trong `MoonlitAudio.cs`, không có file nhạc ngoài.
