# Thiết kế — Cuội: Đường Về Trăng

## Trụ cột

Di chuyển cuộn ngang và nhảy có nhịp. Chó/mèo là nguy hiểm cần né, dụ hoặc làm choáng; không đánh, không loot từ quái, không health bar của quái. Ánh đèn là công cụ đọc địa hình và mở đường. Đường chính rõ; đốm trăng chỉ dẫn và khuyến khích khám phá, không bắt thu thập đủ để qua màn.

## Màn 1 — Rừng Đa

Mục tiêu: đến đỉnh rễ cây và lấy mảnh trăng thứ nhất. Tone: xanh đêm, jade, sương, ánh đèn hổ phách. Mức khó tăng bằng cách kết hợp cơ chế đã gặp, tránh thêm một cơ chế khó hoàn toàn mới ở cuối màn.

| Đoạn / tọa độ X | Ý đồ | Nội dung thực thi |
|---|---|---|
| 0–79: Lối mòn đom đóm | Học chạy, giữ/nhả nhảy và bừng sáng | Bệ rộng, khoảng hở đầu tiên, chó có báo hiệu lao, tuyến bậc thang và đèn checkpoint 1 |
| 79–114: Dòng sương bạc | Quan sát nhịp trước khi nhảy | Bệ lên/xuống, bệ ngang, chỗ nghỉ rộng, mèo có báo hiệu trước khi vồ |
| 114–190: Cầu rễ cổ thụ | Giữ đà, rồi tương tác mở đường | 3 cầu sụp sau 0,7 giây và hồi sau 3,5 giây; checkpoint 2; chó; E thắp đèn mở cầu qua khe |
| 190–274: Tầng rễ trên cao | Tổng hợp kỹ năng đã học, có điểm nghỉ | Checkpoint 3–4, mèo, nhảy lên rồi hạ dần, chó ở bãi rễ, bổ sung tuyến đường để thời lượng gần mục tiêu 2 phút |
| 274–370: Ngọn cây giữ trăng | Chuỗi nhảy cuối cùng | Bệ ngang, mèo, 2 cầu sụp, checkpoint 5, bậc đá đi lên và mảnh trăng ở X=362 |

Gặp thú / rơi vực: mất một sinh lực và hồi tại checkpoint. Hết ba sinh lực được cấp lại ba tại checkpoint để bản prototype không phạt lặp đoạn quá dài. E mở cầu vĩnh viễn cho lượt chơi hiện tại, không reset khi chết. Chơi lại màn reset toàn bộ.

Cuối màn lưu phần thưởng bám vách dành cho màn 2; đây mới là cờ mở khóa và UI, chưa có controller bám vách.

## Hướng cho cả 4 màn — chưa triển khai ngoài màn 1

| Màn | Thời lượng mục tiêu | Nhịp và tương tác | Phần thưởng |
|---|---|---|---|
| 1. Rừng Đa | ~2 phút | Chạy, nhảy, bệ động, cầu sụp, làm choáng, thắp đèn mở cầu | Bám/leo vách cho màn 2 |
| 2. Vách núi sương | ~2 phút | Hai mặt vách gần nhau, bám có giới hạn, rễ cây rút vào theo nhịp, nghỉ tại miếu | Double jump cho màn 3 |
| 3. Hồ trăng vỡ | ~2 phút | Double jump qua lá sen chìm, nâng đèn kích cầu phản chiếu; né mèo trên bệ cao | Lướt gió hoặc tích sáng cho màn 4 |
| 4. Tán Đa trên mây | ~4 phút | Boss môi trường: né rễ quét, nhảy sóng xung kích, thắp 3–4 nút sáng theo chu kỳ để giải phóng cây; không chặt chém | Mảnh cuối, ghép trăng, Cuội và cây Đa trở về |

Boss cuối nên là chuỗi vượt chướng ngại/giải đố có chu kỳ. Mỗi chu kỳ lộ một nút đèn ở vị trí có thể đến được; người chơi dùng kỹ năng di chuyển và E để kích hoạt. Không cần hệ vũ khí hoặc hitbox chém.

## Khả năng thực thi

Màn 1 chỉ cần Rigidbody2D, Collider2D, state machine đơn giản và sprite animator dạng đổi frame. Các tương tác tự khép kín, không phụ thuộc inventory hoặc quest framework. Chưa cần Cinemachine, DOTween hay package ngoài template Unity hiện có. Art có 6 PNG AI; shader glow, hạt và UI dựng bằng code. Khi thay art chỉ cần giữ pivot/PPU và thứ tự animation trong ArtLibrary.

Trước mốc 21/11/2026: ưu tiên playtest nhịp nhảy với người thật, xác nhận thời gian hoàn thành trung bình gần 2 phút, thay frame AI không nhất quán nếu có, thử build trên một máy Windows thứ hai. Không mở thêm phạm vi intro hoặc boss trong giai đoạn polish màn 1.
