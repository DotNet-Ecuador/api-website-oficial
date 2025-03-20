using api.Models;
using MongoDB.Driver;

namespace DotNetEcuador.API.Infraestructure.Services
{
    public class CommunityService
    {
        private readonly IMongoCollection<CommunityMember> _communityCollection;

        public CommunityService(IMongoDatabase database)
        {
            _communityCollection = database.GetCollection<CommunityMember>("community_members");
        }

        public async Task CreateAsync(CommunityMember member)
        {
            await _communityCollection.InsertOneAsync(member);
        }
    }
}
