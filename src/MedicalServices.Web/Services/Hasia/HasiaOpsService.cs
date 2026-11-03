using System.Text.Json;
using MedicalServices.Domain.Entities;
using MedicalServices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedicalServices.Web.Services.Hasia;

public sealed class HasiaOpsService(IDbContextFactory<AppDbContext> dbFactory) : IHasiaOpsService
{
    private const string Fid = HasiaData.FacilityId;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task EnsureSeededAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        if (await db.PrisonInmates.AnyAsync(x => x.FacilityId == Fid, ct))
            return;

        var i1 = MakeInmate("10234", "F-2231", "حمص", "1988-03-12", "O+", "ذكر", "متزوج", "2026-06-02",
            "HTN", "البنسلين",
            [
                new StayDto("سجن حمص", "2025-11-10", "2026-03-01"),
                new StayDto("سجن حماة", "2026-03-01", "2026-06-02"),
                new StayDto("حسيا", "2026-06-02", null)
            ],
            """{"f":{"residence":"مقيم","address":"حمص — الوعر","relative":"شقيق · 0944 000 112","smoking":"نعم — علبة يوميًا","drugs":"أملوديبين 5 ملغ","allergies":"البنسلين","family":"ضغط عند الأب","consent":"نعم","organizer":"رنا العلي","general":"واعٍ، متعاون"},"hist":{"HTN":"منذ 2019"},"sg":{},"vt":{"ضغط الدم":"150/95","الطول":"176","النبض":"82","الوزن":"84","الحرارة":"36.8","سكر الدم":"104","BMI":"27.1","الأكسجة":"97"},"ex":{"القلب والأوعية الدموية":"أصوات قلبية طبيعية","الجهاز التنفسي":"سليم","الأسنان":"نخر ضرس سفلي"},"mk":{"ندبات أو حروق":"ندبة ساعد أيسر"}}""",
            """[{"t":"2026-08-14","d":"ارتفاع ضغط شرياني","by":"د. سامر"}]""");

        var i2 = MakeInmate("10587", "F-2410", "حماة", "1995-11-02", "A+", "ذكر", "عازب", "2026-07-19",
            "سكري", "—", [new StayDto("حسيا", "2026-07-19", null)], null, "[]");

        var i3 = MakeInmate("10912", "F-2588", "دمشق", "1979-01-25", "B-", "ذكر", "متزوج", "2026-09-03",
            "", "—",
            [
                new StayDto("مركز احتجاز عدرا", "2026-05-14", "2026-09-03"),
                new StayDto("حسيا", "2026-09-03", null)
            ], null, "[]");

        db.PrisonInmates.AddRange(i1, i2, i3);

        var today = DateOnly.FromDateTime(DateTime.Now);
        var c301 = new PrisonCase
        {
            FacilityId = Fid, CaseCode = "C-301", InmateNo = "10234", Triager = "م. رامي",
            Complaint = "صداع شديد ودوار", AtServiceId = "p-int", Status = "جارية",
            OpenedTime = "08:40", Day = today,
            Steps =
            [
                new PrisonCaseStep
                {
                    SortOrder = 0, Time = "08:40", AtServiceId = "p-er", By = "م. رامي",
                    Diagnosis = "ارتفاع ضغط", Procedures = "قياس ضغط 170/100", Note = "تحويل ضمن النقطة"
                }
            ]
        };
        var c302 = new PrisonCase
        {
            FacilityId = Fid, CaseCode = "C-302", InmateNo = "10587", Triager = "م. لينا",
            Complaint = "ألم بطني حاد", AtServiceId = "h-surg", Status = "جارية",
            OpenedTime = "09:15", Day = today,
            Steps =
            [
                new PrisonCaseStep
                {
                    SortOrder = 0, Time = "09:15", AtServiceId = "p-er", By = "م. لينا", Note = "تحويل إلى المشفى"
                }
            ]
        };
        var c303 = new PrisonCase
        {
            FacilityId = Fid, CaseCode = "C-303", InmateNo = "10912", Triager = "م. رامي",
            Complaint = "ألم سن", AtServiceId = "p-dent", Status = "منجزة",
            OpenedTime = "07:55", Day = today,
            Steps =
            [
                new PrisonCaseStep { SortOrder = 0, Time = "07:55", AtServiceId = "p-er", By = "م. رامي" },
                new PrisonCaseStep
                {
                    SortOrder = 1, Time = "08:10", AtServiceId = "p-dent", By = "د. نور",
                    Diagnosis = "نخر سني", Procedures = "حشوة مؤقتة", Meds = "باراسيتامول 500 × 6"
                }
            ]
        };
        db.PrisonCases.AddRange(c301, c302, c303);

        // archive samples
        string[] complaints = ["ألم سن", "صداع وحرارة", "جرح في اليد", "ألم صدري", "ضيق تنفس"];
        string[] ats = ["p-dent", "p-int", "p-er", "h-er", "p-er"];
        for (var d = 1; d <= 5; d++)
        {
            var day = today.AddDays(-d);
            for (var k = 0; k < 3; k++)
            {
                var idx = (d + k) % complaints.Length;
                db.PrisonCases.Add(new PrisonCase
                {
                    FacilityId = Fid,
                    CaseCode = $"C-{200 - d * 3 - k}",
                    InmateNo = new[] { "10234", "10587", "10912" }[(d + k) % 3],
                    Triager = k % 2 == 0 ? "م. رامي" : "م. لينا",
                    Complaint = complaints[idx],
                    AtServiceId = ats[idx],
                    Status = "منجزة",
                    OpenedTime = $"{7 + k:00}:{10 + k * 7:00}",
                    Day = day,
                    Steps =
                    [
                        new PrisonCaseStep
                        {
                            SortOrder = 0, Time = $"{7 + k:00}:{10 + k * 7:00}", AtServiceId = ats[idx],
                            By = "د. سامر", Diagnosis = "متابعة", Procedures = "فحص سريري"
                        }
                    ]
                });
            }
        }

        void Stock(string ph, string name, int qty) =>
            db.PrisonPharmacyItems.Add(new PrisonPharmacyItem
            {
                FacilityId = Fid, Pharmacy = ph, DrugName = name, Quantity = qty
            });

        Stock("internal", "باراسيتامول 500", 1200);
        Stock("internal", "إيبوبروفين 400", 640);
        Stock("internal", "أموكسيسيلين 500", 380);
        Stock("internal", "أملوديبين 5", 210);
        Stock("internal", "ميتفورمين 850", 300);
        Stock("internal", "أوميبرازول 20", 45);
        Stock("external", "باراسيتامول 500", 2400);
        Stock("external", "سيفترياكسون 1غ", 90);
        Stock("external", "ديكلوفيناك 75", 160);
        Stock("external", "أملوديبين 5", 520);
        Stock("external", "ترامادول 50", 30);
        Stock("external", "ميترونيدازول 500", 260);

        db.PrisonPrescriptions.Add(new PrisonPrescription
        {
            FacilityId = Fid, Pharmacy = "internal", InmateNo = "10587",
            DrugName = "إيبوبروفين 400", Quantity = 10, Doctor = "د. سامر الحسين",
            CaseCode = "C-304", Status = "pending", CreatedTime = "09:20"
        });

        db.PrisonDispenses.Add(new PrisonDispense
        {
            FacilityId = Fid, Pharmacy = "internal", InmateNo = "10912",
            DrugName = "باراسيتامول 500", Quantity = 6, CaseCode = "C-303",
            By = "د. سامر الحسين", DispensedTime = "08:10"
        });

        await db.SaveChangesAsync(ct);
    }

    private static PrisonInmate MakeInmate(
        string no, string file, string birthPlace, string birthDate, string blood, string sex, string marital,
        string entry, string chronic, string allergies, IReadOnlyList<StayDto> stays, string? recordJson, string diagnosesJson)
    {
        var inmate = new PrisonInmate
        {
            FacilityId = Fid,
            InmateNo = no,
            FileNo = file,
            BirthPlace = birthPlace,
            BirthDate = birthDate,
            BloodType = blood,
            Sex = sex,
            Marital = marital,
            Place = stays[^1].Place,
            EntryDate = entry,
            ChronicCsv = chronic,
            Allergies = allergies,
            RecordJson = recordJson,
            DiagnosesJson = diagnosesJson
        };
        for (var i = 0; i < stays.Count; i++)
        {
            inmate.Stays.Add(new PrisonStay
            {
                SortOrder = i,
                Place = stays[i].Place,
                Entry = stays[i].Entry,
                Exit = stays[i].Exit
            });
        }
        return inmate;
    }

    public async Task<IReadOnlyList<PrisonInmate>> ListInmatesAsync(CancellationToken ct = default)
    {
        await EnsureSeededAsync(ct);
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.PrisonInmates.AsNoTracking()
            .Include(x => x.Stays.OrderBy(s => s.SortOrder))
            .Where(x => x.FacilityId == Fid)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<PrisonInmate?> FindInmateAsync(string inmateNo, CancellationToken ct = default)
    {
        await EnsureSeededAsync(ct);
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.PrisonInmates.AsNoTracking()
            .Include(x => x.Stays.OrderBy(s => s.SortOrder))
            .FirstOrDefaultAsync(x => x.FacilityId == Fid && x.InmateNo == inmateNo, ct);
    }

    public async Task<PrisonInmate> CreateInmateAsync(CreateInmateRequest request, CancellationToken ct = default)
    {
        await EnsureSeededAsync(ct);
        var no = (request.InmateNo ?? "").Trim();
        var file = (request.FileNo ?? "").Trim();
        if (no.Length < 2 || file.Length < 1)
            throw new InvalidOperationException("رقم الملف ورقم النزيل حقول إلزامية.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        if (await db.PrisonInmates.AnyAsync(x => x.FacilityId == Fid && x.InmateNo == no, ct))
            throw new InvalidOperationException("رقم النزيل مسجّل مسبقًا.");

        var stays = request.Stays?.Where(s => !string.IsNullOrWhiteSpace(s.Place)).ToList()
            ?? [new StayDto("حسيا", DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd"), null)];
        var last = stays[^1];

        var inmate = new PrisonInmate
        {
            FacilityId = Fid,
            InmateNo = no,
            FileNo = file,
            Nationality = string.IsNullOrWhiteSpace(request.Nationality) ? "سوري" : request.Nationality.Trim(),
            BirthPlace = request.BirthPlace,
            BirthDate = request.BirthDate,
            BloodType = string.IsNullOrWhiteSpace(request.BloodType) ? "—" : request.BloodType.Trim(),
            Sex = request.Sex is "أنثى" ? "أنثى" : "ذكر",
            Marital = request.Marital ?? "عازب",
            Place = last.Place,
            EntryDate = last.Entry,
            ChronicCsv = request.ChronicCsv ?? "",
            Allergies = string.IsNullOrWhiteSpace(request.Allergies) ? "—" : request.Allergies.Trim(),
            RecordJson = request.RecordJson,
            DiagnosesJson = "[]"
        };
        for (var i = 0; i < stays.Count; i++)
        {
            inmate.Stays.Add(new PrisonStay
            {
                SortOrder = i, Place = stays[i].Place, Entry = stays[i].Entry, Exit = stays[i].Exit
            });
        }

        db.PrisonInmates.Add(inmate);
        await db.SaveChangesAsync(ct);
        return inmate;
    }

    public async Task<IReadOnlyList<PrisonCase>> ListCasesAsync(string? status = null, bool archiveOnly = false, CancellationToken ct = default)
    {
        await EnsureSeededAsync(ct);
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var today = DateOnly.FromDateTime(DateTime.Now);
        var q = db.PrisonCases.AsNoTracking()
            .Include(x => x.Steps.OrderBy(s => s.SortOrder))
            .Where(x => x.FacilityId == Fid);

        if (archiveOnly)
            q = q.Where(x => x.Status == "منجزة" && x.Day < today);
        else
            q = q.Where(x => !(x.Status == "منجزة" && x.Day < today));

        if (!string.IsNullOrWhiteSpace(status) && status != "الكل")
            q = q.Where(x => x.Status == status);

        return await q.OrderByDescending(x => x.CreatedAt).Take(400).ToListAsync(ct);
    }

    public async Task<PrisonCase?> GetCaseAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.PrisonCases.AsNoTracking()
            .Include(x => x.Steps.OrderBy(s => s.SortOrder))
            .FirstOrDefaultAsync(x => x.Id == id && x.FacilityId == Fid, ct);
    }

    public async Task<PrisonCase> OpenTriageCaseAsync(OpenTriageRequest request, CancellationToken ct = default)
    {
        await EnsureSeededAsync(ct);
        if (string.IsNullOrWhiteSpace(request.Triager) || string.IsNullOrWhiteSpace(request.InmateNo) || string.IsNullOrWhiteSpace(request.Complaint))
            throw new InvalidOperationException("أكمل اسم الفارز ورقم النزيل والشكاية.");

        var svc = HasiaData.FindService(request.ToServiceId)
            ?? throw new InvalidOperationException("وجهة التحويل غير صالحة.");
        if (svc.Id.EndsWith("-ph", StringComparison.Ordinal))
            throw new InvalidOperationException("لا يمكن التحويل مباشرة إلى الصيدلية.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        if (!await db.PrisonInmates.AnyAsync(x => x.FacilityId == Fid && x.InmateNo == request.InmateNo.Trim(), ct))
            throw new InvalidOperationException("رقم النزيل غير مسجّل — عبّئ الاستمارة أولًا.");

        var count = await db.PrisonCases.CountAsync(x => x.FacilityId == Fid, ct);
        var now = DateTime.Now;
        var row = new PrisonCase
        {
            FacilityId = Fid,
            CaseCode = $"C-{300 + count + 1}",
            InmateNo = request.InmateNo.Trim(),
            Triager = request.Triager.Trim(),
            Complaint = request.Complaint.Trim(),
            AtServiceId = svc.Id,
            Status = "جارية",
            OpenedTime = now.ToString("HH:mm"),
            Day = DateOnly.FromDateTime(now)
        };
        db.PrisonCases.Add(row);
        await db.SaveChangesAsync(ct);
        return row;
    }

    public async Task<PrisonCase> CompleteStepAsync(Guid caseId, CaseStepRequest request, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var row = await db.PrisonCases.Include(x => x.Steps)
            .FirstOrDefaultAsync(x => x.Id == caseId && x.FacilityId == Fid, ct)
            ?? throw new InvalidOperationException("الحالة غير موجودة.");

        if (row.Status == "منجزة")
            throw new InvalidOperationException("الحالة منجزة مسبقاً.");

        if (string.IsNullOrWhiteSpace(request.Provider))
            throw new InvalidOperationException("أدخل اسم مقدّم الخدمة.");

        var pharmacy = HasiaData.PharmacyOf(row.AtServiceId);
        var medLines = request.Meds ?? [];
        foreach (var m in medLines.Where(x => x.Qty > 0 && !string.IsNullOrWhiteSpace(x.Drug)))
        {
            db.PrisonPrescriptions.Add(new PrisonPrescription
            {
                FacilityId = Fid,
                Pharmacy = pharmacy,
                InmateNo = row.InmateNo,
                DrugName = m.Drug.Trim(),
                Quantity = m.Qty,
                Doctor = request.Provider.Trim(),
                CaseCode = row.CaseCode,
                Status = "pending",
                CreatedTime = DateTime.Now.ToString("HH:mm")
            });
        }

        var note = request.Action switch
        {
            "complete" => "تم إنجاز الحالة",
            "hold" => string.IsNullOrWhiteSpace(request.HoldReason) ? "تعليق" : $"تعليق: {request.HoldReason}",
            "transfer" => request.TargetServiceId is not null && HasiaData.FindService(request.TargetServiceId)?.Zone == "hospital"
                ? "تحويل إلى المشفى"
                : "تحويل ضمن النقطة",
            _ => request.Action
        };

        var step = new PrisonCaseStep
        {
            CaseId = row.Id,
            SortOrder = row.Steps.Count,
            Time = DateTime.Now.ToString("HH:mm"),
            AtServiceId = row.AtServiceId,
            By = request.Provider.Trim(),
            Diagnosis = NullIfEmpty(request.Diagnosis),
            Procedures = NullIfEmpty(request.Procedures),
            Meds = medLines.Count == 0
                ? null
                : string.Join("، ", medLines.Select(m => m.Qty > 0 ? $"{m.Drug} × {m.Qty}" : m.Drug)),
            Note = note
        };
        db.PrisonCaseSteps.Add(step);

        if (request.Action == "complete")
        {
            row.Status = "منجزة";
            row.HoldReason = null;
        }
        else if (request.Action == "hold")
        {
            row.Status = "معلّقة";
            row.HoldReason = request.HoldReason;
        }
        else if (request.Action == "transfer")
        {
            var target = HasiaData.FindService(request.TargetServiceId ?? "")
                ?? throw new InvalidOperationException("وجهة التحويل غير صالحة.");
            row.AtServiceId = target.Id;
            row.Status = "جارية";
            row.HoldReason = null;
        }

        if (!string.IsNullOrWhiteSpace(request.Diagnosis))
        {
            var inmate = await db.PrisonInmates.FirstOrDefaultAsync(x => x.FacilityId == Fid && x.InmateNo == row.InmateNo, ct);
            if (inmate is not null)
            {
                var list = JsonSerializer.Deserialize<List<DiagDto>>(inmate.DiagnosesJson, JsonOpts) ?? [];
                list.Insert(0, new DiagDto(DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd"), request.Diagnosis.Trim(), request.Provider.Trim()));
                inmate.DiagnosesJson = JsonSerializer.Serialize(list, JsonOpts);
            }
        }

        await db.SaveChangesAsync(ct);
        await db.Entry(row).Collection(x => x.Steps).LoadAsync(ct);
        return row;
    }

    public async Task<IReadOnlyList<PrisonPharmacyItem>> ListStockAsync(string pharmacy, CancellationToken ct = default)
    {
        await EnsureSeededAsync(ct);
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.PrisonPharmacyItems.AsNoTracking()
            .Where(x => x.FacilityId == Fid && x.Pharmacy == pharmacy)
            .OrderBy(x => x.DrugName)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PrisonPrescription>> ListPrescriptionsAsync(string pharmacy, bool pendingOnly = true, CancellationToken ct = default)
    {
        await EnsureSeededAsync(ct);
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var q = db.PrisonPrescriptions.AsNoTracking()
            .Where(x => x.FacilityId == Fid && x.Pharmacy == pharmacy);
        if (pendingOnly)
            q = q.Where(x => x.Status == "pending");
        return await q.OrderByDescending(x => x.CreatedAt).Take(100).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PrisonDispense>> ListDispensesAsync(string pharmacy, CancellationToken ct = default)
    {
        await EnsureSeededAsync(ct);
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.PrisonDispenses.AsNoTracking()
            .Where(x => x.FacilityId == Fid && x.Pharmacy == pharmacy)
            .OrderByDescending(x => x.CreatedAt).Take(100).ToListAsync(ct);
    }

    public async Task PrescribeAsync(PrescribeRequest request, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        db.PrisonPrescriptions.Add(new PrisonPrescription
        {
            FacilityId = Fid,
            Pharmacy = request.Pharmacy,
            InmateNo = request.InmateNo.Trim(),
            DrugName = request.DrugName.Trim(),
            Quantity = Math.Max(1, request.Quantity),
            Doctor = request.Doctor.Trim(),
            CaseCode = request.CaseCode,
            Status = "pending",
            CreatedTime = DateTime.Now.ToString("HH:mm")
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task DispenseAsync(DispenseRequest request, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        if (!await db.PrisonInmates.AnyAsync(x => x.FacilityId == Fid && x.InmateNo == request.InmateNo.Trim(), ct))
            throw new InvalidOperationException("رقم النزيل غير مسجّل.");

        var qty = Math.Max(1, request.Quantity);
        var item = await db.PrisonPharmacyItems
            .FirstOrDefaultAsync(x => x.FacilityId == Fid && x.Pharmacy == request.Pharmacy && x.DrugName == request.DrugName.Trim(), ct)
            ?? throw new InvalidOperationException("الدواء غير متوفر في هذه الصيدلية.");

        if (item.Quantity < qty)
            throw new InvalidOperationException($"المخزون غير كافٍ (المتاح {item.Quantity} حبة).");

        item.Quantity -= qty;
        db.PrisonDispenses.Add(new PrisonDispense
        {
            FacilityId = Fid,
            Pharmacy = request.Pharmacy,
            InmateNo = request.InmateNo.Trim(),
            DrugName = item.DrugName,
            Quantity = qty,
            CaseCode = request.CaseCode,
            By = request.By,
            DispensedTime = DateTime.Now.ToString("HH:mm")
        });

        if (request.PrescriptionId is Guid rxId)
        {
            var rx = await db.PrisonPrescriptions.FirstOrDefaultAsync(x => x.Id == rxId && x.FacilityId == Fid, ct);
            if (rx is not null)
                rx.Status = "done";
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task<Dictionary<string, int>> LoadByServiceAsync(CancellationToken ct = default)
    {
        await EnsureSeededAsync(ct);
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var rows = await db.PrisonCases.AsNoTracking()
            .Where(x => x.FacilityId == Fid && x.Status == "جارية")
            .GroupBy(x => x.AtServiceId)
            .Select(g => new { g.Key, N = g.Count() })
            .ToListAsync(ct);
        return rows.ToDictionary(x => x.Key, x => x.N);
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private sealed record DiagDto(string T, string D, string By);
}
