pipeline {
    agent any

    environment {
        SONARQUBE_URL = 'SonarQube' // Name configured in Jenkins
        DOTNET_VERSION = '9.0' // Updated to .NET 9
    }

    stages {
        stage('Checkout') {
            steps {
                git branch: 'master', url: 'https://github.com/SonilalChavhan3/quiz_system.git'
            }
        }

        stage('Install .NET SDK') {
            steps {
                script {
                    def dotnetInstalled = sh(script: 'dotnet --version', returnStatus: true) == 0
                    if (!dotnetInstalled) {
                        error "Dotnet SDK is not installed on this agent!"
                    }
                }
            }
        }

        stage('Restore Dependencies') {
            steps {
                sh 'dotnet restore'
            }
        }

        stage('Build') {
            steps {
                sh 'dotnet build --configuration Release'
            }
        }

        stage('Run Tests') {
            steps {
                sh 'dotnet test --no-build --verbosity normal'
            }
        }

        stage('SonarQube Analysis') {
            steps {
                withSonarQubeEnv(SONARQUBE_URL) {
                    sh '''
                        dotnet sonarscanner begin /k:"quiz_system" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="sqa_b64d9505c1c43eb644aab18f02db192f5ad8686d"
                        dotnet build --configuration Release
                        dotnet sonarscanner end /d:sonar.login="<YOUR_SONARQUBE_TOKEN>"
                    '''
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
