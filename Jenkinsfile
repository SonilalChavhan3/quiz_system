pipeline {
    agent any
    environment {
        DOTNET_ROOT = "C:\\Program Files\\dotnet"
        SOLUTION_NAME = "quiz_system.sln"
        PROJECT_PATH = "quiz_system\\quiz_system.csproj"
        NEXUS_URL = "http://localhost:8081/repository/nuget-hosted-Test/"
        PS_SCRIPT_PATH = ".\\NugetPackagePublish.ps1"
        Project_Name = "quiz_system"
        TestProjectName = "Quiz_System.Tests\\Quiz_System.Tests.csproj"
       // REPORTGEN = "C:\\Users\\HP\\.dotnet\\tools\\reportgenerator.exe"
    }

    stages {
        stage('Checkout') {
            steps {
                echo "[${new Date().format('HH:mm:ss')}] Cleaning workspace..."
                deleteDir()
                checkout scm
            }
        }

        stage('Restore Packages') {
            steps {
                echo "Restoring NuGet packages..."
                bat "dotnet restore ${env.SOLUTION_NAME}"
            }
        }

       stage('SonarQube Analysis') {
    steps {
        script {
            def scannerHome = tool 'SonarScanner for MSBuild'

            withSonarQubeEnv('MySonarQube') {
                // Step 1: Sonar Begin
                bat """
                \"${scannerHome}\\SonarScanner.MSBuild.exe\" begin ^
                    /k:\"${env.Project_Name}_${env.BRANCH_NAME}\" ^
                    /n:\"${env.Project_Name} (${env.BRANCH_NAME})\" ^
                    /v:\"${env.BUILD_NUMBER}\" ^
                    /d:sonar.cs.opencover.reportsPaths=\"**/coverage.opencover.xml\" ^
                    /d:sonar.coverage.exclusions=\"**/*Migrations*/**\"
                """

                // Step 2: Build
                bat "dotnet build ${env.SOLUTION_NAME} -c Release"

                // Step 3: Test with Coverage
                bat """
                dotnet test ${env.SOLUTION_NAME} ^
                    --logger trx ^
                    /p:CollectCoverage=true ^
                    /p:CoverletOutput=TestResults/coverage.opencover.xml ^
                    /p:CoverletOutputFormat=opencover
                """

                // Step 4: Sonar End
                bat "\"${scannerHome}\\SonarScanner.MSBuild.exe\" end"
            }
        }
    }
}


        stage('Build') {
            steps {
                echo "⚙️ Building .NET project..."
                bat "dotnet build ${env.PROJECT_PATH} -c Release --no-restore"
            }
        }

        stage('Test') {
            steps {
                echo 'Testing...'
                // Add your test commands here, for example:
                // bat "dotnet test --no-build --verbosity normal"
            }
        }

        stage('Create and Push NuGet Package') {
            steps {
                script {
                    powershell """
                        powershell.exe -NonInteractive -ExecutionPolicy Bypass `
                        -File \"${env.PS_SCRIPT_PATH}\" `
                        -ProjectName \"${env.Project_Name}\" `
                        -BranchName \"${env.BRANCH_NAME}\" `
                        -BuildNumber \"${env.BUILD_NUMBER}\" `
                        -NexusUrl \"${env.NEXUS_URL}\" 
                    """
                    
                }
            }
        }

        stage('Deploy') {
            steps {
                echo 'Deploying...'
            }
        }
    }
}
