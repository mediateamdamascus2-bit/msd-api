using System.Globalization;
using System.Net;
using System.Text;
using MedicalServices.Domain.Entities;

namespace MedicalServices.Web.Services.Hasia;

public static class HasiaArchiveReport
{
    private static readonly CultureInfo Ar = ArLatnCulture.Instance;

    private static string E(string? s) => WebUtility.HtmlEncode(s ?? "");

    private static string FullDate(DateOnly d) =>
        ArLatnCulture.Format(d.ToDateTime(TimeOnly.MinValue), "dddd d MMMM yyyy");

    public static string BuildDailyHtml(DateOnly day, IReadOnlyList<PrisonCase> cases)
    {
        var list = cases.OrderBy(c => c.OpenedTime).ToList();
        var toHosp = list.Count(c => HasiaData.FindService(c.AtServiceId)?.Zone == "hospital");
        var inPoint = list.Count - toHosp;
        var rx = list.Count(c => c.Steps.Any(s => !string.IsNullOrWhiteSpace(s.Meds)));
        var uniqueInmates = list.Select(c => c.InmateNo).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        var refNo = $"HASIA-ARC-{day:yyyyMMdd}";
        var issued = ArLatnCulture.Format(DateTime.Now, "dddd d MMMM yyyy · HH:mm");
        var dayIso = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var bySvc = list.GroupBy(c => c.AtServiceId)
            .Select(g => (Id: g.Key, N: g.Count()))
            .OrderByDescending(x => x.N)
            .ToList();
        var maxSvc = Math.Max(1, bySvc.Count == 0 ? 1 : bySvc.Max(x => x.N));

        int Pct(int n) => list.Count == 0 ? 0 : (int)Math.Round(100.0 * n / list.Count);

        var hourBuckets = Enumerable.Range(8, 12).Select(h => (H: h, N: 0)).ToArray();
        foreach (var c in list)
        {
            if (!int.TryParse(c.OpenedTime.AsSpan(0, Math.Min(2, c.OpenedTime.Length)), out var h))
                continue;
            var idx = h < 8 ? 0 : h > 19 ? hourBuckets.Length - 1 : h - 8;
            if (idx >= 0 && idx < hourBuckets.Length)
                hourBuckets[idx].N++;
        }
        var maxHour = Math.Max(1, hourBuckets.Max(b => b.N));

        var summary = list.Count == 0
            ? "لا توجد حالات منجزة مؤرشفة لهذا اليوم."
            : $"خلال يوم {E(FullDate(day))} أُنجز {list.Count} حالة طبية لنحو {uniqueInmates} نزيلاً؛ عولج منها {inPoint} ضمن النقطة الطبية ({Pct(inPoint)}%) وحُوّل {toHosp} إلى مشفى السجن ({Pct(toHosp)}%)، وتضمّنت {rx} حالة وصفة دوائية.";

        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html lang='ar' dir='rtl'><head><meta charset='utf-8'/>");
        sb.Append("<title>").Append(E($"تقرير رسمي — أرشيف الحالات · {dayIso}")).Append("</title>");
        sb.Append("""
            <link href="https://fonts.googleapis.com/css2?family=IBM+Plex+Sans+Arabic:wght@400;500;600;700&family=IBM+Plex+Mono:wght@500;600&display=swap" rel="stylesheet"/>
            <style>
            @page{size:A4;margin:12mm 11mm 14mm}
            *{box-sizing:border-box}
            body{margin:0;color:#0f2a30;font-family:"IBM Plex Sans Arabic",Tajawal,Segoe UI,Tahoma,sans-serif;font-size:11px;line-height:1.55;background:#e8eeed}
            .mono{font-family:"IBM Plex Mono",ui-monospace,Consolas,monospace;letter-spacing:.02em}
            .page{max-width:210mm;margin:12px auto;background:#fff;border:1px solid #1c4a54;box-shadow:0 12px 40px rgba(18,56,64,.12);position:relative;overflow:hidden}
            .page::before{content:"";position:absolute;inset:7px;border:1.5px solid #9fb8b4;pointer-events:none;z-index:0}
            .page::after{content:"سريّة طبية · إدارة الخدمات الطبية";position:absolute;inset:0;display:grid;place-items:center;font-size:42px;font-weight:700;color:rgba(28,74,84,.045);transform:rotate(-28deg);letter-spacing:.08em;pointer-events:none;z-index:0}
            .inner{position:relative;z-index:1;padding:18px 20px 16px}
            .ribbon{display:flex;justify-content:space-between;align-items:center;gap:10px;background:#123840;color:#d7efe9;padding:7px 12px;margin:-18px -20px 14px;font-size:10px;font-weight:700;letter-spacing:.04em}
            .ribbon .tag{background:#cf5a52;color:#fff;padding:2px 8px;border-radius:2px;font-size:10px}
            .brand{display:grid;grid-template-columns:70px 1fr 168px;gap:12px;align-items:center;padding-bottom:12px;border-bottom:2.5px solid #1c4a54;margin-bottom:12px}
            .brand img{width:64px;height:64px;object-fit:contain}
            .brand h1{margin:0;font-size:19px;color:#1c4a54;font-weight:700}
            .brand .org{margin:3px 0 0;font-size:12px;color:#3d5f66;font-weight:600}
            .brand .en{margin:2px 0 0;font-size:9.5px;color:#7a9699;letter-spacing:.06em;text-transform:uppercase}
            .meta-box{border:1.5px solid #1c4a54;padding:8px 9px;text-align:center;background:#f3f8f7}
            .meta-box b{display:block;font-size:11px;color:#1c4a54}
            .meta-box span{display:block;font-size:10px;color:#4c6b71;margin-top:2px}
            .doc-title{text-align:center;margin:0 0 12px}
            .doc-title .badge{display:inline-block;background:#1c4a54;color:#fff;padding:4px 14px;font-size:10px;font-weight:700;letter-spacing:.08em;margin-bottom:8px}
            .doc-title h2{margin:0;font-size:17px;color:#123840;font-weight:700}
            .doc-title p{margin:5px 0 0;font-size:12px;color:#4c6b71}
            .ctrl{width:100%;border-collapse:collapse;margin-bottom:12px;font-size:10.5px}
            .ctrl th,.ctrl td{border:1px solid #b7cbc7;padding:5px 8px}
            .ctrl th{background:#eef5f3;color:#1c4a54;width:22%;text-align:right;font-weight:700}
            .kpis{display:grid;grid-template-columns:repeat(5,1fr);gap:7px;margin-bottom:12px}
            .kpi{border:1px solid #c5d4d1;padding:8px 7px;background:linear-gradient(180deg,#f7fbfa,#fff);text-align:center}
            .kpi .v{font-size:20px;font-weight:700;color:#1c4a54;line-height:1.1}
            .kpi .l{font-size:10px;color:#4c6b71;font-weight:600;margin-top:3px}
            .split{display:grid;grid-template-columns:1.15fr .85fr;gap:12px;margin-bottom:4px}
            h3{margin:14px 0 8px;font-size:12.5px;color:#1c4a54;font-weight:700;border-right:3.5px solid #0f8f86;padding-right:8px}
            .summary{margin:0 0 12px;padding:9px 11px;background:#f5f9f8;border:1px solid #c9d9d6;border-right:4px solid #1c4a54;font-size:11.5px;color:#243f45;text-align:justify}
            .bar-row{display:grid;grid-template-columns:1fr 1.4fr 28px 34px;gap:6px;align-items:center;margin:4px 0}
            .bar-lab{font-size:10px;color:#243f45;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
            .bar-track{height:9px;background:#e6efed;border-radius:2px;overflow:hidden}
            .bar-track i{display:block;height:100%;border-radius:2px}
            .bar-n,.bar-p{font-size:10px;color:#1c4a54;font-weight:600;text-align:center}
            .hour-wrap{border:1px solid #c9d9d6;padding:8px 10px 4px;background:#fafcfc}
            .hour-wrap .cap{font-size:10px;color:#4c6b71;font-weight:600;margin-bottom:4px}
            table.data{width:100%;border-collapse:collapse}
            table.data th,table.data td{border:1px solid #c5d4d1;padding:5px 6px;vertical-align:top}
            table.data th{background:#1c4a54;color:#fff;font-size:10px;font-weight:700;text-align:right;white-space:nowrap}
            table.data td{font-size:10.5px}
            table.data tr:nth-child(even) td{background:#f4f8f7}
            .num{text-align:center;direction:ltr}
            .note{margin-top:12px;padding:8px 10px;border:1px dashed #8faba6;color:#4c6b71;font-size:10.5px;background:#fafcfb;text-align:justify}
            .signs{display:grid;grid-template-columns:1fr 1fr 1fr;gap:16px;margin-top:30px;page-break-inside:avoid}
            .sign{text-align:center}
            .sign .line{border-top:1px solid #143139;margin:36px 18px 8px}
            .sign b{display:block;font-size:11.5px;color:#123840}
            .sign span{display:block;font-size:10px;color:#4c6b71;margin-top:2px}
            .footer{margin-top:18px;padding-top:8px;border-top:2px solid #1c4a54;display:flex;justify-content:space-between;gap:10px;font-size:9.5px;color:#4c6b71}
            @media print{body{background:#fff;-webkit-print-color-adjust:exact;print-color-adjust:exact}.page{margin:0;box-shadow:none;max-width:none;border-width:1.5px}.no-print{display:none!important}}
            .toolbar{display:flex;gap:8px;justify-content:center;margin:0 0 14px}
            .toolbar button{border:0;border-radius:6px;padding:10px 18px;font:inherit;font-weight:700;cursor:pointer}
            .toolbar .p{background:#1c4a54;color:#fff}.toolbar .c{background:#e8f0ee;color:#143139}
            </style></head><body>
            """);
        sb.Append("<div class='toolbar no-print'><button class='p' onclick='window.print()'>طباعة / حفظ PDF</button><button class='c' onclick='window.close()'>إغلاق</button></div>");
        sb.Append("<article class='page'><div class='inner'>");
        sb.Append("<div class='ribbon'><span>إدارة الخدمات الطبية · MEDICAL SERVICES DIRECTORATE</span><span class='tag'>سريّة طبية</span></div>");
        sb.Append("<header class='brand'><img src='/images/logo.png' alt=''/><div><h1>إدارة الخدمات الطبية</h1>");
        sb.Append("<p class='org'>سجن حسيا — عيادة السجن الطبية (الدرجة C)</p>");
        sb.Append("<p class='en'>Hasia Prison Medical Clinic · Grade C Facility</p></div>");
        sb.Append("<div class='meta-box'><b>وثيقة رسمية</b><span class='mono'>").Append(E(refNo)).Append("</span><span>نسخة للطباعة / الأرشفة</span></div></header>");
        sb.Append("<div class='doc-title'><div class='badge'>DAILY ARCHIVE REPORT</div>");
        sb.Append("<h2>تقرير أرشيف الحالات الطبية اليومية</h2><p>اليوم التشغيلي: <strong>").Append(E(FullDate(day))).Append("</strong></p></div>");
        sb.Append("<table class='ctrl'>");
        sb.Append("<tr><th>رقم الوثيقة</th><td class='mono'>").Append(E(refNo)).Append("</td><th>تاريخ الإصدار</th><td>").Append(E(issued)).Append("</td></tr>");
        sb.Append("<tr><th>الجهة المصدِرة</th><td>عيادة سجن حسيا</td><th>التصنيف</th><td>سريّة طبية — للاستخدام الداخلي</td></tr>");
        sb.Append("<tr><th>نطاق التقرير</th><td colspan='3'>جميع الحالات المنجزة والمؤرشفة ليوم ").Append(E(dayIso)).Append(" (YYYY-MM-DD)</td></tr></table>");
        sb.Append("<div class='kpis'>");
        sb.Append("<div class='kpi'><div class='v mono num'>").Append(list.Count).Append("</div><div class='l'>إجمالي الحالات</div></div>");
        sb.Append("<div class='kpi'><div class='v mono num'>").Append(uniqueInmates).Append("</div><div class='l'>نزلاء فريدون</div></div>");
        sb.Append("<div class='kpi'><div class='v mono num'>").Append(inPoint).Append("</div><div class='l'>ضمن النقطة</div></div>");
        sb.Append("<div class='kpi'><div class='v mono num'>").Append(toHosp).Append("</div><div class='l'>حُوّلت للمشفى</div></div>");
        sb.Append("<div class='kpi'><div class='v mono num'>").Append(rx).Append("</div><div class='l'>وصفات دواء</div></div></div>");
        sb.Append("<h3>الملخص التنفيذي</h3><p class='summary'>").Append(summary).Append("</p>");
        sb.Append("<div class='split'><div><h3 style='margin-top:0'>توزّع الحالات حسب الجهة</h3>");
        foreach (var s in bySvc)
        {
            var svc = HasiaData.FindService(s.Id);
            var w = Math.Max(6, (int)Math.Round(100.0 * s.N / maxSvc));
            var fill = svc?.Zone == "hospital" ? "#3a7ba6" : "#0f8f86";
            sb.Append("<div class='bar-row'><span class='bar-lab'>").Append(E(svc?.Name ?? s.Id))
                .Append("</span><div class='bar-track'><i style='width:").Append(w).Append("%;background:").Append(fill)
                .Append("'></i></div><span class='mono num bar-n'>").Append(s.N)
                .Append("</span><span class='mono num bar-p'>").Append(Pct(s.N)).Append("%</span></div>");
        }
        if (bySvc.Count == 0) sb.Append("<div class='note'>لا بيانات للتوزّع</div>");
        sb.Append("</div><div><h3 style='margin-top:0'>كثافة الحالات حسب الساعة</h3><div class='hour-wrap'>");
        sb.Append("<div class='cap'>من 08:00 إلى 19:00 · عدد الحالات لكل ساعة</div>");
        const int hourSvgW = 520, gap = 6;
        var bw = (hourSvgW - gap * (hourBuckets.Length - 1)) / (double)hourBuckets.Length;
        sb.Append("<svg viewBox='0 0 520 78' width='100%' height='78' aria-hidden='true'>");
        for (var i = 0; i < hourBuckets.Length; i++)
        {
            var b = hourBuckets[i];
            var h = Math.Max(b.N > 0 ? 4 : 0, (int)Math.Round(52.0 * b.N / maxHour));
            var x = i * (bw + gap);
            var y = 58 - h;
            var fill = b.N > 0 ? "#1c4a54" : "#d9e6e3";
            sb.Append("<rect x='").Append(x.ToString("0.##", CultureInfo.InvariantCulture))
                .Append("' y='").Append(y).Append("' width='").Append(bw.ToString("0.##", CultureInfo.InvariantCulture))
                .Append("' height='").Append(h).Append("' rx='2' fill='").Append(fill).Append("'/>");
            sb.Append("<text x='").Append((x + bw / 2).ToString("0.##", CultureInfo.InvariantCulture))
                .Append("' y='72' text-anchor='middle' font-size='8' fill='#4c6b71' font-family='Consolas,monospace'>")
                .Append(b.H.ToString("00")).Append("</text>");
        }
        sb.Append("</svg></div></div></div>");
        sb.Append("<h3>جدول التوزّع العددي</h3><table class='data'><thead><tr><th>الجهة / القسم</th><th>المنطقة</th><th>العدد</th><th>النسبة</th></tr></thead><tbody>");
        foreach (var s in bySvc)
        {
            var svc = HasiaData.FindService(s.Id);
            sb.Append("<tr><td>").Append(E(svc?.Name ?? s.Id)).Append("</td><td>")
                .Append(E(svc is null ? "—" : HasiaData.ZoneLabel(svc.Zone)))
                .Append("</td><td class='mono num'>").Append(s.N)
                .Append("</td><td class='mono num'>").Append(Pct(s.N)).Append("%</td></tr>");
        }
        if (bySvc.Count == 0) sb.Append("<tr><td colspan='4'>لا بيانات</td></tr>");
        sb.Append("</tbody></table>");
        sb.Append("<h3>السجل التفصيلي للحالات</h3><table class='data'><thead><tr><th>#</th><th>رقم الحالة</th><th>رقم النزيل</th><th>الوقت</th><th>الشكاية</th><th>التشخيص</th><th>الجهة النهائية</th><th>الفارز</th><th>الوصفة</th></tr></thead><tbody>");
        var iRow = 1;
        foreach (var c in list)
        {
            var last = c.Steps.OrderBy(x => x.SortOrder).LastOrDefault();
            var svc = HasiaData.FindService(c.AtServiceId);
            var dest = svc is null ? c.AtServiceId : $"{HasiaData.ZoneLabel(svc.Zone)} · {svc.Name}";
            sb.Append("<tr><td class='num'>").Append(iRow++).Append("</td><td class='mono'>").Append(E(c.CaseCode))
                .Append("</td><td class='mono'>#").Append(E(c.InmateNo))
                .Append("</td><td class='mono num'>").Append(E(c.OpenedTime))
                .Append("</td><td>").Append(E(c.Complaint))
                .Append("</td><td>").Append(E(last?.Diagnosis ?? "—"))
                .Append("</td><td>").Append(E(dest))
                .Append("</td><td>").Append(E(c.Triager))
                .Append("</td><td>").Append(E(last?.Meds ?? "—"))
                .Append("</td></tr>");
        }
        if (list.Count == 0) sb.Append("<tr><td colspan='9'>لا توجد حالات لهذا اليوم</td></tr>");
        sb.Append("</tbody></table>");
        sb.Append("<div class='note'>هذه الوثيقة صادرة آلياً من منظومة عيادة سجن حسيا وتُعدّ سجلاً رسمياً للمتابعة الطبية الداخلية. الحالات المدرجة منجزة ومؤرشفة للقراءة فقط؛ أي تصحيح يتم عبر مسار الحالات الجارية قبل الإغلاق اليومي. يُحفظ التقرير مع ختم الوحدة وتوقيع المسؤول الطبي ضمن أرشيف العيادة الورقي/الإلكتروني.</div>");
        sb.Append("<div class='signs'><div class='sign'><div class='line'></div><b>المسؤول الطبي للسجن</b><span>الاسم والتوقيع</span></div>");
        sb.Append("<div class='sign'><div class='line'></div><b>رئيس النقطة الطبية</b><span>الاسم والتوقيع</span></div>");
        sb.Append("<div class='sign'><div class='line'></div><b>ختم الوحدة الطبية</b><span>التاريخ: ").Append(E(dayIso)).Append("</span></div></div>");
        sb.Append("<footer class='footer'><span>تاريخ الإصدار: ").Append(E(issued))
            .Append("</span><span class='mono'>").Append(E(refNo))
            .Append("</span><span>إدارة الخدمات الطبية — سجن حسيا · نهاية الوثيقة</span></footer>");
        sb.Append("</div></article>");
        sb.Append("<script>window.addEventListener('load',()=>setTimeout(()=>window.print(),400));</script></body></html>");
        return sb.ToString();
    }
}
