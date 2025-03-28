pipeline {
    agent any

    environment {
        SONARQUBE_URL = 'SonarQube' // Name configured in Jenkins
        DOTNET_VERSION = '9.0' // Updated to .NET 9
        SONARQUBE_TOKEN = credentials('sonarqubesecret') // Use Jenkins credentials store
    }

    stages {
        stage('Checkout') {
            steps {
                git branch: 'master', url: 'https://github.com/SonilalChavhan3/quiz_system.git'
            }
        }

        stage('Verify .NET SDK') {
            steps {
               script {
            def dotnetVersion = bat(script: 'dotnet --version', returnStdout: true).trim().split("\r?\n")[-1] // Get last line
            if (!dotnetVersion.startsWith(DOTNET_VERSION)) {
                error "Expected .NET version $DOTNET_VERSION, but found $dotnetVersion!"
            } else {
                echo "Verified .NET version: $dotnetVersion"
                    }
                }
            }
        }

        stage('Restore Dependencies') {
            steps {
                bat 'dotnet restore'
            }
        }

        stage('Build') {
            steps {
                bat 'dotnet build --configuration Release'
            }
        }

        stage('Run Tests') {
            steps {
                bat 'dotnet test --no-build --verbosity normal'
            }
        }

        stage('SonarQube Analysis') {
            steps {
                withSonarQubeEnv(SONARQUBE_URL) {
                    bat """
                        dotnet sonarscanner begin /k:"quiz_system" /d:sonar.host.url="http://113.193.63.106:55017" /d:sonar.login="%SONARQUBE_TOKEN%"
                        dotnet build --configuration Release
                        dotnet sonarscanner end /d:sonar.login="%SONARQUBE_TOKEN%"
                    """
                }
            }
        }

        stage('Quality Gate') {
            steps {
                timeout(time: 1, unit: 'MINUTES') {
                    waitForQualityGate abortPipeline: true
                }
            }
        }
    }
}
