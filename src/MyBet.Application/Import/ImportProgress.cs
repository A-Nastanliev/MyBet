namespace MyBet.Application.Import
{
    public record ImportProgress(int Done, int Total, ImportResult Result);
}