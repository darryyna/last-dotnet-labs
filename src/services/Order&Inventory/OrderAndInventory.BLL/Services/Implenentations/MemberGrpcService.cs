using Grpc.Core;
using OrderAndInventory.DAL.Repositories.Interfaces;
using OrderAndInventory.Grpc;

namespace OrderAndInventory.BLL.Services.Implenentations;

public class MemberGrpcService : MemberGRPCService.MemberGRPCServiceBase
{
    private readonly IMemberRepository _memberRepository;
    
    public MemberGrpcService(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }
    
    public override async Task<DoesMemberExistResponse> DoesMemberExist(
        DoesMemberExistRequest request,
        ServerCallContext context)
    {
        var member = await _memberRepository.GetByIdAsync(
            Guid.Parse(request.Id),
            cancellationToken: context.CancellationToken
        );

        return new DoesMemberExistResponse
        {
            Exists = member != null
        };
    }
}