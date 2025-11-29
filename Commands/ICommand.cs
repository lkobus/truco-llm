namespace truco_net.Commands;

public interface ICommand
{
    Task Execute(Mediator mediator);
}
