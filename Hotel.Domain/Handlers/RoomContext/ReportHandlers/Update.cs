using Hotel.Domain.DTOs;
using Hotel.Domain.DTOs.RoomContext.ReportDTOs;

namespace Hotel.Domain.Handlers.RoomContext.ReportHandlers;

public partial class ReportHandler 
{
  public async Task<Response> HandleUpdateAsync(UpdateReport model, Guid id)
  {
    var report = await _repository.GetEntityByIdAsync(id);
    if (report == null)
      throw new ArgumentException("Relatório não encontrado.");

    report.ChangeSummary(model.Summary);
    report.ChangeDescription(model.Description);
    report.ChangePriority(model.Priority);
    report.ChangeResolution(model.Resolution);


    _repository.Update(report);
    await _repository.SaveChangesAsync();

    return new Response(200,"Relatório atualizado com sucesso!.",new { report.Id });
  }
}