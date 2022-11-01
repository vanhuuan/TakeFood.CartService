using TakeFood.CartService.ViewModel.Dtos.Address;

namespace TakeFood.CartService.Service
{
    public interface IAddressService
    {
        Task CreateAddress(AddressDto address);
        Task DeleteAddress(String id);
    }
}
