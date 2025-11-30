pipeline {
    agent any
    environment {
        DOTNET_ROOT = "C:\\Program Files\\dotnet"
        SOLUTION_NAME = "quiz_system.sln"
        PROJECT_PATH = "quiz_system\\quiz_system.csproj"
        NEXUS_URL = "http://localhost:8081/repository/nuget-hosted-Test/"
        //PS_SCRIPT_PATH = "C:\\Tools\\commonbuild\\NugetPackagePublish.ps1"
         PS_SCRIPT_PATH = ".\\NugetPackagePublish.ps1"
        Project_Name = "quiz_system"
        TestProjectName = "Quiz_System.Tests\\Quiz_System.Tests.csproj"
        
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
         stage('Test') {
            steps {
               // echo "Running unit tests..."
                 //   bat "dotnet test ${TestProjectName} --configuration Release --no-build"
               // bat "dotnet test ${env.SOLUTION_NAME} --configuration Release --no-build"

                 echo "Running unit tests + Coverage..."

        bat """
            dotnet test ${env.TestProjectName} ^
                --configuration Release ^
                --no-build ^
                --collect "XPlat Code Coverage"
        """
            }
        }
        stage('Coverage Report') {
            steps {
                bat """
                    reportgenerator ^
                        "-reports:**/coverage.cobertura.xml" ^
                        "-targetdir:coverage-report" ^
                        "-reporttypes:Cobertura"
                """
            }
        }
        stage('SonarQube Analysis') {
            steps {
                script {
                    // Assign tool inside script block
                    def scannerHome = tool 'SonarScanner for MSBuild'

                    // Use withSonarQubeEnv inside script block
                    withSonarQubeEnv('MySonarQube') {
                        bat """
                    \"${scannerHome}\\SonarScanner.MSBuild.exe\" begin ^
                    /k:\"${env.Project_Name}_${env.BRANCH_NAME}\" ^
                    /n:\"${env.Project_Name} (${env.BRANCH_NAME})\" ^
                    /v:\"${env.BUILD_NUMBER}\" ^
                    /d:sonar.cs.vscoveragexml.reportsPaths="coverage-report\\Cobertura.xml" ^
                    /d:sonar.coverage.exclusions="**/bin/**,**/obj/**"
                """
                        bat "dotnet build ${env.SOLUTION_NAME} -c Release"
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
