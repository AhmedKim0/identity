namespace Identity.Application.DTO.UserDTOs
{
    public class UpdateUserDTO
    {
        public int Id { get; set; }
        public string NewEmail { get; set; }
        public string NewPassword { get; set; }
        public string NewFullName { get; set; }
    }
}
