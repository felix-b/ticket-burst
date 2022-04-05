import React from 'react'
import { Box, Center, Flex, Grid, GridItem, } from "@chakra-ui/react"

import { HeaderBar } from '../components/headerBar'
import { HeroEventCard } from '../components/heroEventCard'
import { EventSearchResult } from '../contracts/backendApi'
import { Utility } from '../utility'
import { EventCard } from '../components/eventCard'
import { EventSearchForm } from '../components/eventSearchForm'
import { ServiceClient } from '../serviceClient'

interface IndexPageProps {
    events: EventSearchResult[]
}

const IndexPage = (props: IndexPageProps) => {

    const events = props.events.map(Utility.eventSearchResultToCardProps);
    const allCardsRendered: JSX.Element[] = [0,1,2,3,4,5,6,7,8].map(index => {
        switch (index) {
            case 0: return events.length > 1 
                ? <EventCard key={index} {...events[1]} />
                : null
            case 1: return <EventSearchForm key={index} />
            default: return index < events.length ? 
                <EventCard key={index} {...events[index]} /> 
                : null
        }
    })

    return (
        <>
            <HeaderBar />
            <Grid 
                minH="100vh"
                bg="white" 
                templateRows="500px auto"
                templateColumns="auto 1200px auto"
            >
                <GridItem  colSpan={3} bg="blue.800" pt="100px" >
                    <Center>
                        <Box w={1200}  mt='40px'>
                            {events[0] && <HeroEventCard {...events[0]} />}
                        </Box>
                    </Center>
                </GridItem>
                <GridItem  h="full"></GridItem>
                <GridItem  h="full" minH={1000}>
                    <Flex mt='40px' wrap='wrap' justify='space-between' gap='40px'>
                        {allCardsRendered}
                    </Flex>
                </GridItem>
                <GridItem  h="full"></GridItem>
            </Grid>  
        </>
    )
}

export default IndexPage

export async function getServerSideProps(): Promise<{ props: IndexPageProps }> {
    try {
        const res = await fetch(`${ServiceClient.getServiceUrl('search')}/search?selling=true&count=9`)
        const envelope = await res.json()
        const events = envelope.data as EventSearchResult[]
        return { 
            props: { 
                events 
            }
        }
    } catch(err) {
        console.log(err)
        return { 
            props: {
                events: []
            }
        }
    }
}
