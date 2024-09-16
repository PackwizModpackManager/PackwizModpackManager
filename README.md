# PackwizModpackManager

> Works with all Modloaders, it can edit the version of the loader too.
> 
> *It can goes wrong creating a project, that's not fault of the PackwizModpackManager*
>
> It throw's it from Packwiz Itself, i cannot bugfix it, it's from HTTPS connection!

## Prerequisites

Before starting, ensure you have the following requirements installed:

- **.NET SDK 8.0**: You can download it from the [official .NET site](https://dotnet.microsoft.com/download/dotnet/8.0).
- **Visual Studio 2022** or any other IDE compatible with .NET 8.0 (optional).
- **Git**: If you don't have Git installed, you can get it [here](https://git-scm.com/).

## Cloning the Project

To clone the repository to your local machine, run the following command in your terminal:

```
git clone https://github.com/user/project.git
```

Replace `https://github.com/user/project.git` with the correct URL of your repository.

## Building the Project

Once the repository is cloned, follow these steps to build the project:

1. Open a terminal and navigate to the project directory:

   ```
   cd path/to/project
   ```

2. Restore the necessary project dependencies using the following command:

   ```
   dotnet restore
   ```

3. Build the project with the following command:

   ```
   dotnet build
   ```

This will generate the binaries in the `bin/Debug/net8.0/` folder.

## Running the Project

To run the project after building it, use the following command:

```
dotnet run
```

This will run the project in the development environment.

## Building for Production

If you want to build the project for a production environment, use the following command:

```
dotnet publish --configuration Release --output ./published
```

This will generate an optimized build in the `./published` folder.

## Contributing

If you'd like to contribute to this project, follow these steps:

1. Fork the repository.
2. Create a branch for your new feature:

   ```
   git checkout -b my-new-feature
   ```

3. Make your changes and commit them:

   ```
   git commit -m "Added new feature"
   ```

4. Push the changes to your forked repository:

   ```bash
   git push origin my-new-feature
   ```

5. Open a Pull Request in this repository to review your changes.
