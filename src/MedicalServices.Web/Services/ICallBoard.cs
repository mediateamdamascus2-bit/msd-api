namespace MedicalServices.Web.Services;

public sealed record PatientCallEvent(
    string FacilityId,
    Guid CaseId,
    string CaseNumber,
    string PatientName,
    string Clinic,
    int CallCount,
    DateTimeOffset At);

public interface ICallBoard
{
    event Action<PatientCallEvent>? CallReceived;

    PatientCallEvent? GetCurrent(string facilityId);

    IReadOnlyList<PatientCallEvent> GetRecent(string facilityId, int take = 8);

    void Publish(PatientCallEvent call);
}

public sealed class CallBoard : ICallBoard
{
    private readonly object _gate = new();
    private readonly Dictionary<string, PatientCallEvent> _current = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<PatientCallEvent>> _history = new(StringComparer.OrdinalIgnoreCase);

    public event Action<PatientCallEvent>? CallReceived;

    public PatientCallEvent? GetCurrent(string facilityId)
    {
        lock (_gate)
            return _current.GetValueOrDefault(facilityId);
    }

    public IReadOnlyList<PatientCallEvent> GetRecent(string facilityId, int take = 8)
    {
        lock (_gate)
        {
            if (!_history.TryGetValue(facilityId, out var list))
                return [];
            return list.Take(Math.Max(1, take)).ToList();
        }
    }

    public void Publish(PatientCallEvent call)
    {
        lock (_gate)
        {
            if (_current.TryGetValue(call.FacilityId, out var prev) && prev.CaseId != call.CaseId)
            {
                if (!_history.TryGetValue(call.FacilityId, out var list))
                {
                    list = [];
                    _history[call.FacilityId] = list;
                }

                list.Insert(0, prev);
                while (list.Count > 20)
                    list.RemoveAt(list.Count - 1);
            }

            _current[call.FacilityId] = call;
        }

        CallReceived?.Invoke(call);
    }
}
