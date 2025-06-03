using System;
using DummyProject.Types;

namespace DummyProject.Models.ViewModel;

public class NavbarModel
{


    public Role UserRole { get; set; }
    public bool IsLoggedin { get; set; }
    public Guid UserId { get; set; }

}